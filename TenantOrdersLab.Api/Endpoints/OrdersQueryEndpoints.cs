using Microsoft.AspNetCore.Mvc;
using TenantOrdersLab.Api.Common;
using TenantOrdersLab.App.Order.Queries.GetOrderById;
using TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

namespace TenantOrdersLab.Api.Endpoints;

public static class OrdersRequestEndpoints
{
    public static IEndpointRouteBuilder MapOrdersReadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        // ✅ GET /api/orders/{id}
        // Note: route constraint should match the parameter type (int here).
        group.MapGet("/{id:int}", GetByIdAsync)
            .WithName("GetOrderById")
            .Produces<OrderDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        // ✅ GET /api/orders/by-customer/{customerId}
        group.MapGet("/by-customer/{customerId:int}", ListByCustomerAsync)
            .WithName("ListOrdersByCustomer")
            .Produces<List<OrderListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> GetByIdAsync(
        int id,
         GetOrderByIdHandler handler,
        HttpContext http,
        CancellationToken ct)
    {
        if (id <= 0)
        {

            return ApiProblemFactory.ToProblemResult("validation: Id must be greater than 0.", http);
        }
        var query = new GetOrderByIdQuery(id);
        var result = await handler.HandleAsync(query, ct);

        // If your handler returns Result<OrderDetailsDto>, this maps ok/notfound
        return result.ToHttpResult(http);
    }

    private static async Task<IResult> ListByCustomerAsync(
        int customerId,
         ListOrdersByCustomerHandler handler,
        HttpContext http,
        CancellationToken ct)
    {
        // Minimal APIs already enforce {customerId:int}, but keeping this is fine for clearer errors.
        if (customerId <= 0)
        {
            return ApiProblemFactory.ToProblemResult(
        "validation: customerId must be greater than 0.",
        http);
        }

        var query = new ListOrdersByCustomerQuery(customerId);
        var result = await handler.HandleAsync(query, ct);

        return result.ToHttpResult(http);
    }
}
