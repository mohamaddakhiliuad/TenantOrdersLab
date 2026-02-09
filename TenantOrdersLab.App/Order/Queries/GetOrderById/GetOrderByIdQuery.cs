namespace TenantOrdersLab.App.Order.Queries.GetOrderById;

/// <summary>
/// Query: Get a single order details view by OrderId.
/// Read-only operation.
/// </summary>
public sealed record GetOrderByIdQuery(int OrderId);
