using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Persistence;

namespace TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

/// <summary>
/// Handler for listing orders by customer.
/// </summary>
public sealed class ListOrdersByCustomerHandler
{
    private readonly IOrderReadQueries _queries;

    public ListOrdersByCustomerHandler(IOrderReadQueries queries)
    {
        _queries = queries;
    }

    public async Task<Result<IReadOnlyList<OrderListItemDto>>> HandleAsync(
        ListOrdersByCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.CustomerId <= 0)
            return Result<IReadOnlyList<OrderListItemDto>>.Failure("Invalid CustomerId.");

        var items = await _queries.ListOrdersByCustomerAsync(query.CustomerId, cancellationToken);
        return Result<IReadOnlyList<OrderListItemDto>>.Success(items);
    }
}
