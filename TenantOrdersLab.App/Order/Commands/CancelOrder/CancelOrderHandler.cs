using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Persistence;

namespace TenantOrdersLab.App.Order.Commands.CancelOrder;

/// <summary>
/// Use case: CancelOrder
/// - Loads Order aggregate for update
/// - Applies domain transition (Cancel)
/// - Commits changes
/// </summary>
public sealed class CancelOrderHandler
{
    private readonly IOrdersUnitOfWork _uow;

    public CancelOrderHandler(IOrdersUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<CancelOrderResult>> HandleAsync(
        CancelOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.OrderId <= 0)
            return Result<CancelOrderResult>.Failure("validation: Invalid OrderId.");

        var order = await _uow.Orders.GetForUpdateAsync(command.OrderId, cancellationToken);
        if (order is null)
            return Result<CancelOrderResult>.Failure("not_found: Order not found.");

        // Domain behavior (business rule lives in the aggregate)
        order.Cancel(command.Reason);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result<CancelOrderResult>.Success(new CancelOrderResult(order.Id));
    }
}
