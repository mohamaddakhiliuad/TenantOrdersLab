using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TenantOrdersLab.Api.Middleware;


namespace TenantOrdersLab.Api.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {

        services.AddTransient<GlobalExceptionMiddleware>();
      
;
        return services;
    }
}
