namespace TenantOrdersLab.Api.Contracts.Orders
{
    public sealed record CompleteOrderRequest(int OrderId, string ExpectedRowVersion);
}
