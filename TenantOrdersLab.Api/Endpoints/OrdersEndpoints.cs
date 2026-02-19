using TenantOrdersLab.Api.Common;
using TenantOrdersLab.Api.Contracts.Orders;
using TenantOrdersLab.App.Order.Commands.CancelOrder;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.App.Orders.Commands.CreateOrder;

namespace TenantOrdersLab.Api.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/cancel", CancelOrder)
            .WithName("CancelOrder")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderRequest request,
        CreateOrderHandler handler,
        HttpContext http,
        CancellationToken ct)
    {
        // You can add lightweight API validation here if needed, but ideally keep it in App.
        var cmd = new CreateOrderCommand(request.CustomerId, request.TotalAmount, request.Currency);

        var result = await handler.HandleAsync(cmd, ct);

        return result.ToHttpResult(http, onSuccess: r =>
            Results.Created($"/api/orders/{r.OrderId}", r));
    }

    private static async Task<IResult> CancelOrder(
        CancelOrderRequest request,
        CancelOrderHandler handler,
        HttpContext http,
        CancellationToken ct)
    {
        var cmd = new CancelOrderCommand(request.OrderId, request.Reason);
        var result = await handler.HandleAsync(cmd, ct);

        return result.ToHttpResult(http);
    }
}
