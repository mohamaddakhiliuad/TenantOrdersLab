using System;
using TenantOrdersLab.Domain.Abstractions;

namespace TenantOrdersLab.Domain.Events
{
    /// <summary>
    /// Raised when an order is placed (created/confirmed) in the domain.
    /// </summary>
    public sealed record OrderPlaced(
        int OrderId,
        int CustomerId,
        Guid EventId,
        DateTime OccurredAtUtc
    ) : IDomainEvent
    {
        public OrderPlaced(int OrderId, int CustomerId)
            : this(OrderId, CustomerId, Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }
}
