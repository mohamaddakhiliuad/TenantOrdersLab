using System;

namespace TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

/// <summary>
/// Lightweight list item DTO for orders.
/// </summary>
public sealed record OrderListItemDto
{
    public int OrderId { get; init; }
    public string Status { get; init; } = string.Empty;

    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public DateTime? CreatedAtUtc { get; init; }
}
