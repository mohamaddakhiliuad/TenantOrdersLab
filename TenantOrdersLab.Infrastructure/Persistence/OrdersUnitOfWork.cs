using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Persistence;

namespace TenantOrdersLab.Infrastructure.Persistence;

internal sealed class OrdersUnitOfWork : IOrdersUnitOfWork
{
    private readonly OrdersDbContext _db;

    public OrdersUnitOfWork(OrdersDbContext db, IOrderRepository orders, ICustomerRepository customers)
    {
        _db = db;
        Orders = orders;
        Customers = customers;
    }

    public IOrderRepository Orders { get; }
    public ICustomerRepository Customers { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
