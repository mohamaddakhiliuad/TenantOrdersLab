namespace TenantOrdersLab.Api.Contracts.Orders
{
    public sealed record ShipOrderRequest(int OrderId, string ExpectedRowVersion);
   
}
