using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Events;
using TenantOrdersLab.Domain.Abstractions;
using TenantOrdersLab.Domain.Entities;

namespace TenantOrdersLab.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for persistence concerns only.
/// - Applies tenant scoping via global query filters (ITenantScoped)
/// - Applies auditing (IAudited) + tenant shadow properties in SaveChanges pipeline
/// - Dispatches domain events AFTER successful commit (Phase 2 decision)
/// </summary>
public sealed class OrdersDbContext : DbContext
{
    private const string TenantIdShadow = "TenantId";
    private const string CreatedAtUtcShadow = "CreatedAtUtc";
    private const string UpdatedAtUtcShadow = "UpdatedAtUtc";

    private static readonly MethodInfo SetTenantFilterOpenGeneric =
        typeof(OrdersDbContext).GetMethod(nameof(SetTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"Missing method: {nameof(SetTenantFilter)}");

    private readonly ITenantProvider _tenantProvider;
    private readonly IClock _clock;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public string CurrentTenantId => _tenantProvider.TenantId;

    // ✅ Only one constructor: prevents "silent degraded mode"
    public OrdersDbContext(
        DbContextOptions<OrdersDbContext> options,
        ITenantProvider tenantProvider,
        IClock clock,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);

        // ✅ Tenant filter is always on (Phase 2 safety)
        ApplyTenantQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyPersistencePolicies();
        var result = base.SaveChanges();
        DispatchDomainEventsSync();
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyPersistencePolicies();
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    // -----------------------------
    // Tenant Query Filter (GENERIC)
    // -----------------------------
    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(ITenantScoped).IsAssignableFrom(clrType))
                continue;

            // Build: SetTenantFilter<TEntity>(modelBuilder)
            var closed = SetTenantFilterOpenGeneric.MakeGenericMethod(clrType);
            closed.Invoke(this, new object[] { modelBuilder });
        }
    }

    /// <summary>
    /// Strongly-typed filter so EF can translate + model-cache correctly.
    /// Captures CurrentTenantId (per DbContext instance / per request).
    /// </summary>
    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => EF.Property<string>(e, TenantIdShadow) == CurrentTenantId);
    }

    // -----------------------------
    // SaveChanges Policy (GENERIC)
    // -----------------------------
    private void ApplyPersistencePolicies()
    {
        var now = _clock.UtcNow;
        var tenantId = _tenantProvider.TenantId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                continue;

            ApplyTenantPolicy(entry, tenantId);
            ApplyAuditPolicy(entry, now);
        }
    }

    private static void ApplyTenantPolicy(EntityEntry entry, string tenantId)
    {
        if (entry.Entity is not ITenantScoped)
            return;

        // Guard: only set if shadow property exists in model
        var tenantProp = entry.Metadata.FindProperty(TenantIdShadow);
        if (tenantProp is null)
            return;

        var tenantEntryProp = entry.Property(TenantIdShadow);

        // Always enforce tenant for new entities
        if (entry.State == EntityState.Added)
        {
            tenantEntryProp.CurrentValue = tenantId;
            return;
        }

        // For modified entities: enforce tenant AND prevent tenant switching
        tenantEntryProp.CurrentValue = tenantId;
        tenantEntryProp.IsModified = false;
    }

    private static void ApplyAuditPolicy(EntityEntry entry, DateTime nowUtc)
    {
        if (entry.Entity is not IAudited)
            return;

        // Guard: only set if shadow properties exist in model
        var createdProp = entry.Metadata.FindProperty(CreatedAtUtcShadow);
        var updatedProp = entry.Metadata.FindProperty(UpdatedAtUtcShadow);

        if (entry.State == EntityState.Added)
        {
            if (createdProp is not null) entry.Property(CreatedAtUtcShadow).CurrentValue = nowUtc;
            if (updatedProp is not null) entry.Property(UpdatedAtUtcShadow).CurrentValue = nowUtc;
            return;
        }

        if (entry.State == EntityState.Modified)
        {
            if (updatedProp is not null) entry.Property(UpdatedAtUtcShadow).CurrentValue = nowUtc;

            // Optional hardening: never allow CreatedAtUtc to be modified
            if (createdProp is not null) entry.Property(CreatedAtUtcShadow).IsModified = false;
        }
    }

    // -----------------------------
    // Domain Events (after commit)
    // -----------------------------
    private void DispatchDomainEventsSync()
    {
        var entities = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .ToList();

        foreach (var entity in entities)
        {
            var events = entity.PullDomainEvents();
            foreach (var ev in events)
                _domainEventDispatcher.Dispatch(ev);
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entities = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .ToList();

        foreach (var entity in entities)
        {
            var events = entity.PullDomainEvents();
            foreach (var ev in events)
                await _domainEventDispatcher.DispatchAsync(ev, ct).ConfigureAwait(false);
        }
    }
}
