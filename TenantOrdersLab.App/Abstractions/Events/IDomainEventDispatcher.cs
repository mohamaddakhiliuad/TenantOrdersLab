using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.Domain.Events;

namespace TenantOrdersLab.App.Abstractions.Events;

public interface IDomainEventDispatcher
{
    void Dispatch(IDomainEvent domainEvent);
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct);
}
