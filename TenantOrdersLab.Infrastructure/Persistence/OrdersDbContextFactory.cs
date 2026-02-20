using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Events;
using TenantOrdersLab.Domain.Events;

namespace TenantOrdersLab.Infrastructure.Persistence;

public sealed class OrdersDbContextFactory
    : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=TenantOrdersLab;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        // Fake dependencies for design-time
        var tenantProvider = new DesignTimeTenantProvider();
        var clock = new DesignTimeClock();
        var dispatcher = new DesignTimeDomainEventDispatcher();

        return new OrdersDbContext(options, tenantProvider, clock, dispatcher);
    }
}

internal sealed class DesignTimeTenantProvider : ITenantProvider
{
    public string TenantId => "design-time";
}

internal sealed class DesignTimeClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

internal sealed class DesignTimeDomainEventDispatcher : IDomainEventDispatcher
{
    public void Dispatch(object domainEvent) { }

    public void Dispatch(IDomainEvent domainEvent)
    {
       // throw new NotImplementedException();
    }

    public Task DispatchAsync(object domainEvent, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        return Task.CompletedTask; 
    }
}