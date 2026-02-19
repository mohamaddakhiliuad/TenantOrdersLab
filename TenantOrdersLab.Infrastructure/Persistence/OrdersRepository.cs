using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.Domain.Entities;

namespace TenantOrdersLab.Infrastructure.Persistence;

public sealed class OrdersRepository : IOrderRepository
{
    private readonly OrdersDbContext _db;

    public OrdersRepository(OrdersDbContext db) => _db = db;

    public void Add(Order order)
    {
        _db.Orders.Add(order);
    }

    public Task<Order?> GetForUpdateAsync(int orderId, CancellationToken cancellationToken = default)
    {
       return _db.Orders.AsTracking().FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    // اینجا متدهای اینترفیس را پیاده کن
}
