using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TenantOrdersLab.Api.Middleware;
using TenantOrdersLab.Api.Validators.Orders;


namespace TenantOrdersLab.Api.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {

        services.AddTransient<GlobalExceptionMiddleware>();
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
       

        ;
        return services;
    }
}
