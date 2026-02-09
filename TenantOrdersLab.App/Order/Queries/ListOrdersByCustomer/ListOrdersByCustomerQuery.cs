namespace TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

/// <summary>
/// Query: List orders for a customer (read-only).
/// </summary>
public sealed record ListOrdersByCustomerQuery(int CustomerId);
