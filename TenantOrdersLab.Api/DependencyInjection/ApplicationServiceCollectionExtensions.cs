using Microsoft.Extensions.DependencyInjection;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.App.Order.Commands.CancelOrder;

namespace TenantOrdersLab.App.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use Cases / Handlers
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<CancelOrderHandler>();

        return services;
    }
}
