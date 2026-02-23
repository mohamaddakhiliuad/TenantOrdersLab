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
public sealed class EfOrderReadQueries : IOrderReadQueries
{
    private readonly OrdersDbContext _db;

    public EfOrderReadQueries(OrdersDbContext db)
    {
        _db = db;
    }

    
      public async Task<OrderDetailsDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var row = await _db.Orders
            .AsNoTracking()
            .TagWith("Query: GetOrderById")
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.CustomerId,
                Status = o.Status.ToString(),
                TotalAmount = o.Total.Amount,
                Currency = o.Total.Currency,
                CreatedAtUtc = EF.Property<System.DateTime?>(o, "CreatedAtUtc"),
                UpdatedAtUtc = EF.Property<System.DateTime?>(o, "UpdatedAtUtc"),

                // ✅ now REAL property on entity
                o.RowVersion
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        return new OrderDetailsDto
        {
            OrderId = row.Id,
            CustomerId = row.CustomerId,
            Status = row.Status,
            TotalAmount = row.TotalAmount,
            Currency = row.Currency,
            CreatedAtUtc = row.CreatedAtUtc,
            UpdatedAtUtc = row.UpdatedAtUtc,

            // ✅ Base64 outside SQL
            RowVersion = Convert.ToBase64String(row.RowVersion)
        };
    }


    public async Task<IReadOnlyList<OrderListItemDto>> ListOrdersByCustomerAsync(
     int customerId,
     CancellationToken cancellationToken = default)
    {
        var rows = await _db.Orders
            .AsNoTracking()
            .TagWith("Query: ListOrdersByCustomer")
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id,
                Status = o.Status.ToString(),
                TotalAmount = o.Total.Amount,
                Currency = o.Total.Currency,
                CreatedAtUtc = EF.Property<System.DateTime?>(o, "CreatedAtUtc"),
                o.RowVersion
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new OrderListItemDto
        {
            OrderId = r.Id,
            Status = r.Status,
            TotalAmount = r.TotalAmount,
            Currency = r.Currency,
            CreatedAtUtc = r.CreatedAtUtc,
            RowVersion = Convert.ToBase64String(r.RowVersion)
        }).ToList();
    }
    }