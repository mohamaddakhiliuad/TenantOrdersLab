using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Events;
using TenantOrdersLab.Domain.Abstractions;

namespace TenantOrdersLab.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for persistence concerns only.
/// - Applies tenant scoping via global query filters (ITenantScoped)
/// - Applies auditing (IAudited) + tenant shadow properties in SaveChanges pipeline
/// - Dispatches domain events AFTER successful commit (Phase 2 decision)
/// </summary>
public sealed class OrdersDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IClock _clock;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

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
        var tenantId = _tenantProvider.TenantId;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            // EF.Property<string>(e, "TenantId") == tenantId
            var left = Expression.Call(
                typeof(EF),
                nameof(EF.Property),
                new[] { typeof(string) },
                parameter,
                Expression.Constant("TenantId"));

            var right = Expression.Constant(tenantId);
            var body = Expression.Equal(left, right);
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
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

            // TenantId for all ITenantScoped
            if (entry.Entity is ITenantScoped)
            {
                entry.Property("TenantId").CurrentValue = tenantId;
            }

            // Auditing for all IAudited
            if (entry.Entity is IAudited)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAtUtc").CurrentValue = now;
                    entry.Property("UpdatedAtUtc").CurrentValue = now;
                }
                else
                {
                    entry.Property("UpdatedAtUtc").CurrentValue = now;
                }
            }
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
