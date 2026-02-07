using System.Collections.Generic;
using TenantOrdersLab.Domain.Events;

namespace TenantOrdersLab.Domain.Abstractions;

/// <summary>
/// Implemented by entities/aggregates that produce domain events.
/// Events are pulled after a successful commit.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> PullDomainEvents();
}
