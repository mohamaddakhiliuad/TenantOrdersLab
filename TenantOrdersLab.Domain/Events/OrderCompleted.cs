namespace TenantOrdersLab.Domain.Events
{
    public record OrderCompleted(int OrderID,
        Guid EventId,
      DateTime OccurredAtUtc) : IDomainEvent
    {
        public OrderCompleted(int OrderID) : this(OrderID, Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }

}