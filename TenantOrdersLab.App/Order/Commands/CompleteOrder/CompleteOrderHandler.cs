
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Persistence;
namespace TenantOrdersLab.App.Order.Commands.CompleteOrder
{
    // <summary>
    /// Use case: CompleteOrder
    /// - Loads Order aggregate for update
    /// - Applies domain transition (Complete)
    /// - Commits changes
    /// </summary>
    public class CompleteOrderHandler
    {
        public readonly IOrdersUnitOfWork _uow;
        public CompleteOrderHandler(IOrdersUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<CompleteOrderResult>> HandleAsync(CompleteOrderCommand command, CancellationToken cancellationToken = default)
        {
            if (command.OrderId <= 0)
                return Result<CompleteOrderResult>.Failure("Invalid OrderId.");
            var order = await _uow.Orders.GetForUpdateAsync(command.OrderId, cancellationToken);
            if (order is null)
                return Result<CompleteOrderResult>.Failure("Order not found.");
            order.Complete();
            await _uow.SaveChangesAsync();
            return Result<CompleteOrderResult>.Success(new CompleteOrderResult(order.Id));
        }

    }
}
