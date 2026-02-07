using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions;
using TenantOrdersLab.App.Orders.Commands.PlaceOrder;
using TenantOrdersLab.App.Abstractions;
using TenantOrdersLab.App.Abstractions.Common;

namespace TenantOrdersLab.App.Orders.Commands.PlaceOrder
{
    /// <summary>
    /// Use case: PlaceOrder
    /// - Loads order (write-side, tracked)
    /// - Calls domain behavior: order.Place()
    /// - Commits
    /// - Collects domain events AFTER successful commit (dispatch comes later in Phase 3/4)
    /// </summary>
    public sealed class PlaceOrderHandler
    {
        private readonly BadCopyOrdersDbContext _db;

        public PlaceOrderHandler(BadCopyOrdersDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PlaceOrderResult>> HandleAsync(
            PlaceOrderCommand command,
            CancellationToken cancellationToken = default)
        {
            var order = await _db.GetOrderForUpdateAsync(command.OrderId, cancellationToken);
            if (order is null)
                return Result<PlaceOrderResult>.Failure("Order not found.");

            // Domain transition (no persistence logic here)
            order.Place();

            await _db.SaveChangesAsync(cancellationToken);

            // Collect domain events AFTER commit (implementation depends on your Domain base class)
            // If you have order.PullDomainEvents() => use it.
            var eventCount = order.PullDomainEvents().Count; //

            return Result<PlaceOrderResult>.Success(new PlaceOrderResult(eventCount));
        }
    }
}
