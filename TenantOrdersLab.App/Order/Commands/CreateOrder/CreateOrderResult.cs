using System;

namespace TenantOrdersLab.App.Order.Commands.CreateOrder
{
    /// <summary>
    /// Output boundary for CreateOrder use case.
    /// </summary>
    public sealed record CreateOrderResult(int OrderId);
}
