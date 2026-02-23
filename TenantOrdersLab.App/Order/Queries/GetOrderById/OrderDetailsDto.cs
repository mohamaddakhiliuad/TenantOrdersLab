using System;

namespace TenantOrdersLab.App.Order.Queries.GetOrderById;

/// <summary>
/// Read model (DTO) for Order details.
/// Keep it flat and UI-friendly.
/// </summary>
public sealed record OrderDetailsDto
{
    public int OrderId { get; init; }
    public int CustomerId { get; init; }

    public string Status { get; init; } = string.Empty;

    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public DateTime? CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public string RowVersion { get; init; } = default!;
}
