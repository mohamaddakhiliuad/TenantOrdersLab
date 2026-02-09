using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Order.Queries.GetOrderById;
using TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

namespace TenantOrdersLab.App.Abstractions.Persistence;

/// <summary>
/// Read-side query boundary for Orders.
/// 
/// Goals:
/// - Keep EF Core / IQueryable out of the Application layer
/// - Force projection to DTOs (fast, safe, no graph tracking)
/// - Enable Infrastructure to apply AsNoTracking / TagWith / tenant filters
/// </summary>
public interface IOrderReadQueries
{
    Task<OrderDetailsDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderListItemDto>> ListOrdersByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default);
}
