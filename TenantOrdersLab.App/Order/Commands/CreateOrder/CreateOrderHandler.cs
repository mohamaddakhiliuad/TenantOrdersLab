using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
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

        public CreateOrderHandler( IOrdersUnitOfWork uow, IIdempotencyStore idempotency, ITenantProvider tenantProvider)
        {
            _uow = uow;
            _idempotency = idempotency;
            _tenantProvider = tenantProvider;
        }
        private readonly IIdempotencyStore _idempotency;
        private readonly ITenantProvider _tenantProvider;

        public async Task<Result<CreateOrderResult>> HandleAsync(
            CreateOrderCommand command,
            CancellationToken cancellationToken = default)
        {

            var tenantId = _tenantProvider.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<CreateOrderResult>.Failure("failure: Missing tenant context.");

            if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
                return Result<CreateOrderResult>.Failure("validation: Idempotency-Key is required.");

            var now = DateTime.UtcNow;

            // stable request hash
            var requestHash = IdempotencyHashing.HashCreateOrder(
                command.CustomerId,
                command.TotalAmount,
                command.Currency);

            // 1️⃣ Ask IdempotencyStore what to do
            var decision = await _idempotency.TryBeginAsync(
                tenantId,
                command.IdempotencyKey,
                requestHash,
                now,
                TimeSpan.FromDays(2),
                cancellationToken);

            if (decision.HasConflict)
                return Result<CreateOrderResult>.Failure("conflict: Idempotency-Key reuse with different payload.");

            if (decision.IsDuplicate && decision.ExistingOrderId.HasValue)
                return Result<CreateOrderResult>.Success(
                    new CreateOrderResult(decision.ExistingOrderId.Value));

            if (decision.IsInProgress)
                return Result<CreateOrderResult>.Failure(
                    "conflict: Request with this Idempotency-Key is already in progress.");

            // 2️⃣ Normal business logic continues

            var customer = await _uow.Customers
                .GetForUpdateAsync(command.CustomerId, cancellationToken);

            if (customer is null)
                return Result<CreateOrderResult>.Failure("not_found: Customer not found.");

            var total = Money.Of(command.TotalAmount, command.Currency);

            var order = Domain.Entities.Order.CreateNew(customer.Id, total);

            _uow.Orders.Add(order);

            await _uow.SaveChangesAsync(cancellationToken);
            // 3️⃣ Mark idempotency as completed
            await _idempotency.CompleteAsync(
                tenantId,
                command.IdempotencyKey,
                order.Id,
                cancellationToken);

            return Result<CreateOrderResult>.Success(
                new CreateOrderResult(order.Id));
        }
    }
}
