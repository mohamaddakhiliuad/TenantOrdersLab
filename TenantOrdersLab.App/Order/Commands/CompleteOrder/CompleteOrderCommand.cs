using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenantOrdersLab.App.Order.Commands.CompleteOrder
{
    /// <summary>
    /// Input boundary for completing an Order (transition Placed -> Completed).
    /// Use case: CompleteOrder
    /// 
    public record CompleteOrderCommand (int OrderId);
    
}
