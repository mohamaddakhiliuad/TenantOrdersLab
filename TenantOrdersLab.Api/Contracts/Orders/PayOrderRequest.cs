namespace TenantOrdersLab.Api.Contracts.Orders

{
    public sealed record PayOrderRequest(int OrderId, string ExpectedRowVersion);
    
}
