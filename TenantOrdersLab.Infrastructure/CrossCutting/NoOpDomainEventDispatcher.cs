using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Events;
using TenantOrdersLab.Domain.Events;

namespace TenantOrdersLab.Infrastructure;

public sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public void Dispatch(IDomainEvent domainEvent)
    {
        // Intentionally empty.
        // In production, this would publish to an event bus.
    }

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Intentionally empty.
        return Task.CompletedTask;
    }
}
