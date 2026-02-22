using Microsoft.Extensions.DependencyInjection;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.App.Order.Commands.CancelOrder;
using TenantOrdersLab.App.Orders.Commands.PlaceOrder;
using TenantOrdersLab.App.Order.Queries.GetOrderById;
using TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;

namespace TenantOrdersLab.App.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use Cases / Handlers
        //Command
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<CancelOrderHandler>();
        services.AddScoped<PlaceOrderHandler>();
        //Query
        services.AddScoped<GetOrderByIdHandler>();
        services.AddScoped< ListOrdersByCustomerHandler>();

        return services;
    }
}
