namespace TenantOrdersLab.Api.Contracts.Orders;

public sealed record CancelOrderRequest(
    int OrderId,
    string Reason,
     string ExpectedRowVersion
);
