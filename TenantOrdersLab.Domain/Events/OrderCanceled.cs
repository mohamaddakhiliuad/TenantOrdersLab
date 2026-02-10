using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.Domain.Events
{
    public record OrderCanceled(int OrderId,
      string Reason,
      Guid EventId,
      DateTime OccurredAtUtc) : IDomainEvent
    {
        public OrderCanceled(int OrderId, string Reason)
           : this(OrderId, Reason, Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }
}
   

  
