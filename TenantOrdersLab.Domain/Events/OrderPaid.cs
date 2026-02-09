using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.Domain.Events
{
    public record OrderPaid(
         int OrderId,
        decimal Amount,
        Guid EventId,
        DateTime OccurredAtUtc) : IDomainEvent
    {
        public OrderPaid(int OrderId, decimal Amount)
           : this(OrderId, Amount, Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }
}
