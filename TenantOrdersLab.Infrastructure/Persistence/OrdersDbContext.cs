using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.Domain.Abstractions;
using TenantOrdersLab.Domain.Events;

namespace TenantOrdersLab.Infrastructure.Persistence
{
    public sealed class OrdersDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public OrdersDbContext(
            DbContextOptions<OrdersDbContext> options,
            ITenantProvider tenantProvider,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            : base(options)
        {
            _tenantProvider = tenantProvider;
            _clock = clock;
            _domainEventDispatcher = domainEventDispatcher;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);

            if (_tenantProvider is not null)
                // 3.4 — Global Tenant Query Filter (GENERIC)
                ApplyTenantQueryFilters(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            // 3.5 — SaveChanges pipeline (GENERIC)
            ApplyPersistencePolicies();
            var result = base.SaveChanges();
            DispatchDomainEventsSync();
            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyPersistencePolicies();
            var result = await base.SaveChangesAsync(cancellationToken);
            await DispatchDomainEventsAsync(cancellationToken);
            return result;
        }

        // -----------------------------
        // 3.4 — GENERIC Tenant Filter
        // -----------------------------
        private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
        {
            if (_tenantProvider is null) return; // ✅ مهم
            var tenantId = _tenantProvider.TenantId;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
                    continue;

                // (e) => EF.Property<string>(e, "TenantId") == tenantId
                var parameter = Expression.Parameter(entityType.ClrType, "e");

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
        // 3.5 — GENERIC SaveChanges Policy
        // -----------------------------
        private void ApplyPersistencePolicies()
        {
            if (_tenantProvider is null || _clock is null) return;
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
                        // CreatedAt stays unchanged
                        entry.Property("UpdatedAtUtc").CurrentValue = now;
                    }
                }
            }
        }

        // -----------------------------
        // Domain Events — after commit
        // -----------------------------
        private void DispatchDomainEventsSync()
        {
            // Any entity that has PullDomainEvents()
            var entities = ChangeTracker.Entries()
                .Select(e => e.Entity)
                .OfType<IHasDomainEvents>()   // پیشنهاد: این interface رو در Domain.Abstractions بساز
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
                    await _domainEventDispatcher.DispatchAsync(ev, ct);
            }
        }
    }

    // پیشنهاد خیلی تمیز برای جنریک کردن Domain Events:
    // این interface را در Domain.Abstractions بگذار و Order implement کند.
    public interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> PullDomainEvents();
    }
    public interface ITenantProvider
    {
        string TenantId { get; }
    }

    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public interface IDomainEventDispatcher
    {
        void Dispatch(IDomainEvent domainEvent);
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct);
    }

}
