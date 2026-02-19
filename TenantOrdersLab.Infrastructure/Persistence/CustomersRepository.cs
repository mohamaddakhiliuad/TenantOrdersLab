using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.Domain.Entities;

namespace TenantOrdersLab.Infrastructure.Persistence;

public sealed class CustomersRepository : ICustomerRepository
{
    private readonly OrdersDbContext _db;

    public CustomersRepository(OrdersDbContext db) => _db = db;

    public void Add(Customer customer)
    {
        // Tracked by EF; will be inserted on UnitOfWork.SaveChangesAsync()
        _db.Customers.Add(customer);
    }

    public Task<Customer?> GetForUpdateAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        // ForUpdate => return a tracked entity (default tracking is fine).
        // If you have a TenantId filter, it should be applied here too.
        return _db.Customers
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
    }
}
