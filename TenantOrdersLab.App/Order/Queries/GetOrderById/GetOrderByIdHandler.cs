using System.Threading;
using System.Threading.Tasks;
using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Persistence;

namespace TenantOrdersLab.App.Order.Queries.GetOrderById;

/// <summary>
/// Handler for GetOrderById query.
/// Uses read-side abstraction to keep EF out of Application.
/// </summary>
public sealed class GetOrderByIdHandler
{
    private readonly IOrderReadQueries _queries;

    public GetOrderByIdHandler(IOrderReadQueries queries)
    {
        _queries = queries;
    }

    public async Task<Result<OrderDetailsDto>> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.OrderId <= 0)
            return Result<OrderDetailsDto>.Failure("Invalid OrderId.");

        var dto = await _queries.GetOrderByIdAsync(query.OrderId, cancellationToken);
        return dto is null
            ? Result<OrderDetailsDto>.Failure("Order not found.")
            : Result<OrderDetailsDto>.Success(dto);
    }
}
