using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.App.Orders.Commands.CreateOrder;
using TenantOrdersLab.Domain.ValueObjects;
namespace TenantOrdersLab.App.Order.Commands.CreateOrder
{
    /// <summary>
    /// Use case: CreateOrder
    /// - Validates the customer exists (tenant-safe; enforced by Infrastructure)
    /// - Creates a new Order aggregate
    /// - Commits changes as a single transaction boundary
    /// </summary>
    public sealed class CreateOrderHandler
    {
        private readonly IOrdersUnitOfWork _uow;

        public CreateOrderHandler(IOrdersUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<CreateOrderResult>> HandleAsync(
            CreateOrderCommand command,
            CancellationToken cancellationToken = default)
        {
            // 1) Validate customer exists (write-side retrieval)
            var customer = await _uow.Customers.GetForUpdateAsync(command.CustomerId, cancellationToken);
            if (customer is null)
                return Result<CreateOrderResult>.Failure("Customer not found.");

            // 2) Create domain value object(s)
            var total = Money.Of(command.TotalAmount, command.Currency);

            // 3) Create order (domain behavior / factory)
            var order = Domain.Entities.Order.CreateNew(customer.Id, total);

            // 4) Persist new aggregate
            _uow.Orders.Add(order);

            // 5) Commit (transaction boundary)
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<CreateOrderResult>.Success(new CreateOrderResult(order.Id));
        }
    }
}
