using Microsoft.Extensions.DependencyInjection;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.App.Order.Commands.CancelOrder;
using TenantOrdersLab.App.Orders.Commands.PlaceOrder;
using TenantOrdersLab.App.Order.Queries.GetOrderById;
using TenantOrdersLab.App.Order.Queries.ListOrdersByCustomer;
using TenantOrdersLab.Api.Validators.Orders;
using FluentValidation;


namespace TenantOrdersLab.App.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use Cases / Handlers
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
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
