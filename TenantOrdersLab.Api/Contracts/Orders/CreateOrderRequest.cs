namespace TenantOrdersLab.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    int CustomerId,
    decimal TotalAmount,
    string Currency
);
