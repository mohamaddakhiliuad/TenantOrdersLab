using TenantOrdersLab.Api.Common;
using TenantOrdersLab.Api.Contracts.Orders;
using TenantOrdersLab.App.Order.Commands.CancelOrder;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.App.Orders.Commands.PlaceOrder;
using Microsoft.AspNetCore.OpenApi;

namespace TenantOrdersLab.Api.Endpoints;

public static class OrdersCommandsEndpoints
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
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(op =>
            {
                op.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    Name = "Idempotency-Key",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Required = true,
                    Description = "Unique key to ensure idempotent POST requests",
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
                });
                return op;
            });



        group.MapPost("/PlaceOrder", PlaceOrder)
           .WithName("PlaceOrder")
           .Produces(StatusCodes.Status200OK)
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
        var idemKey = http.Request.Headers["Idempotency-Key"].ToString();
        if (string.IsNullOrWhiteSpace(idemKey))
            return ApiProblemFactory.ToProblemResult("validation: Idempotency-Key header is required.", http);

       // var cmd = new CreateOrderCommand(request.CustomerId, request.TotalAmount, request.Currency, idemKey);
        // You can add lightweight API validation here if needed, but ideally keep it in App.
        var cmd = new CreateOrderCommand(request.CustomerId, request.TotalAmount, request.Currency, idemKey);

        var result = await handler.HandleAsync(cmd, ct);

        return result.ToHttpResult(http, onSuccess: r =>
            Results.Created($"/api/orders/{r.OrderId}", r));
    }
    private static async Task<IResult> PlaceOrder(
       PlaceOrderRequest request,
      PlaceOrderHandler handler,
       HttpContext http,
       CancellationToken ct)
    {
        // You can add lightweight API validation here if needed, but ideally keep it in App.
        var cmd = new PlaceOrderCommand(request.OrderID);

        var result = await handler.HandleAsync(cmd, ct);

        return result.ToHttpResult(http);
          
    }

    private static async Task<IResult> CancelOrder(
        CancelOrderRequest request,
        CancelOrderHandler handler,
        HttpContext http,
        CancellationToken ct)
    {
        var cmd = new CancelOrderCommand(request.OrderId, request.Reason,request.ExpectedRowVersion);
        var result = await handler.HandleAsync(cmd, ct);

        return result.ToHttpResult(http);
    }
}
