using System;
using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions;
using TenantOrdersLab.App.Common;
using TenantOrdersLab.App.Orders.Commands.CreateOrder;
using TenantOrdersLab.App.Abstractions;
using TenantOrdersLab.App.Common;
using TenantOrdersLab.Domain;

namespace TenantOrdersLab.App.Orders.Commands.CreateOrder
{
    /// <summary>
    /// Use case: CreateOrder
    /// - Validates customer exists (tenant-safe, enforced by Infrastructure query filter / tenant boundary)
    /// - Creates Order in New status
    /// - Commits changes
    /// </summary>
    public sealed class CreateOrderHandler
    {
        private readonly IOrdersDbContext _db;

        public CreateOrderHandler(IOrdersDbContext db)
        {
            _db = db;
        }

        public async Task<Result<CreateOrderResult>> HandleAsync(
            CreateOrderCommand command,
            CancellationToken cancellationToken = default)
        {
            // 1) Validate customer exists (write-side retrieval)
            var customer = await _db.GetCustomerForUpdateAsync(command.CustomerId, cancellationToken);
            if (customer is null)
                return Result<CreateOrderResult>.Failure("Customer not found.");

            // 2) Create domain value object(s)
            // NOTE: adjust Money ctor/factory to match your Domain implementation
            var total = Money.Of(command.TotalAmount, command.Currency);

            // 3) Create order (domain)
            // NOTE: adjust Order constructor/factory to match your Domain implementation
            var order = Order.CreateNew(customer.Id, total);

            // 4) Persist (Infrastructure will implement AddOrder)
            _db.AddOrder(order);

            // 5) Commit
            await _db.SaveChangesAsync(cancellationToken);

            return Result<CreateOrderResult>.Success(new CreateOrderResult(order.Id));
        }
    }
}   
