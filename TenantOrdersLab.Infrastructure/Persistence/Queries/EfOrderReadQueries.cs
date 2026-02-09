using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.App.Order.Queries.GetOrderById;
using TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

namespace TenantOrdersLab.Infrastructure.Persistence.Queries;

/// <summary>
/// EF Core implementation of read-side queries.
/// - Uses AsNoTracking for performance/safety
/// - Projects into DTOs (no Includes / no entity graphs)
/// - Tenant isolation is enforced by global query filters in OrdersDbContext
/// </summary>
internal sealed class EfOrderReadQueries : IOrderReadQueries
{
    private readonly OrdersDbContext _db;

    public EfOrderReadQueries(OrdersDbContext db)
    {
        _db = db;
    }

    public Task<OrderDetailsDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return _db.Orders
            .AsNoTracking()
            .TagWith("Query: GetOrderById")
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDetailsDto
            {
                OrderId = o.Id,
                CustomerId = o.CustomerId,
                Status = o.Status.ToString(),        // adjust if Status is string
                TotalAmount = o.Total.Amount,        // adjust to your Money model
                Currency = o.Total.Currency,         // adjust to your Money model
                CreatedAtUtc = EF.Property<System.DateTime?>(o, "CreatedAtUtc"),
                UpdatedAtUtc = EF.Property<System.DateTime?>(o, "UpdatedAtUtc"),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderListItemDto>> ListOrdersByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var list = await _db.Orders
            .AsNoTracking()
            .TagWith("Query: ListOrdersByCustomer")
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.Id)
            .Select(o => new OrderListItemDto
            {
                OrderId = o.Id,
                Status = o.Status.ToString(),        // adjust if needed
                TotalAmount = o.Total.Amount,        // adjust
                Currency = o.Total.Currency,         // adjust
                CreatedAtUtc = EF.Property<System.DateTime?>(o, "CreatedAtUtc"),
            })
            .ToListAsync(cancellationToken);

        return list;
    }
}
