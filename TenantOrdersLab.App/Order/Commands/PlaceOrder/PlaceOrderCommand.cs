using System;

namespace TenantOrdersLab.App.Orders.Commands.PlaceOrder
{
    /// <summary>
    /// Input boundary for placing an Order (transition New -> Placed).
    /// </summary>
    public sealed record PlaceOrderCommand(int OrderId);
}
