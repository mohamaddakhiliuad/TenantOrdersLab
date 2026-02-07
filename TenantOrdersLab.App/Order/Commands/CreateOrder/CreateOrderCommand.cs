using System;

namespace TenantOrdersLab.App.Orders.Commands.CreateOrder
{
    /// <summary>
    /// Input boundary for creating a new Order (write side).
    /// </summary>
    public sealed record CreateOrderCommand(
        int CustomerId,
        decimal TotalAmount,
        string Currency
    );
}
