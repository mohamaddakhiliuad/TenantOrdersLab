using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.Domain.Events
{
    public record OrderCreated(
        int OrderId,
        Guid EventId,
        DateTime OccurredAtUtc) : IDomainEvent
    {
        public OrderCreated(int OrderId)
           : this(OrderId, Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }
    
}
