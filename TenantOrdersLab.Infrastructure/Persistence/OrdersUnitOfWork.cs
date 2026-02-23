using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.Domain.Entities;

namespace TenantOrdersLab.Infrastructure.Persistence;

public sealed class OrdersUnitOfWork : IOrdersUnitOfWork
{
    private readonly OrdersDbContext _db;

    public OrdersUnitOfWork(OrdersDbContext db, IOrderRepository orderRepository, ICustomerRepository customerRepository )
    {
        _db = db;

        _db = db ?? throw new ArgumentNullException(nameof(db));
        Orders = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        Customers = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    public IOrderRepository Orders { get; }
    public ICustomerRepository Customers { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);

    public void SetOriginalRowVersion(Order order, string expectedRowVersion)
    {
        _db.Entry(order)
       .Property(x => x.RowVersion)
       .OriginalValue =
           Convert.FromBase64String(expectedRowVersion);
    }
}
