using System;

namespace TenantOrdersLab.App.Orders.Commands.CreateOrder
{
    /// <summary>
    /// Output boundary for CreateOrder use case.
    /// </summary>
    public sealed record CreateOrderResult(int OrderId);
}
