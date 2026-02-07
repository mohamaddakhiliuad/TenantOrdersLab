namespace TenantOrdersLab.App.Orders.Commands.PlaceOrder
{
    /// <summary>
    /// Output boundary for PlaceOrder.
    /// EventCount is optional ("events collected after commit").
    /// </summary>
    public sealed record PlaceOrderResult(int DomainEventCount);
}
