using System;

namespace TenantOrdersLab.Domain.Events
{
    /// <summary>
    /// Contract for domain events raised by aggregates.
    /// Domain events are immutable facts about something that already happened in the domain.
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredAtUtc { get; }
    }
}
