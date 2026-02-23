using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TenantOrdersLab.App.Abstractions.Common;
using TenantOrdersLab.App.Abstractions.Events;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.Infrastructure.Persistence;
using TenantOrdersLab.Infrastructure.Persistence.Idempotency;
using TenantOrdersLab.Infrastructure.Persistence.Queries;


namespace TenantOrdersLab.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Needed for HttpTenantProvider (reads headers)
        //services.AddHttpContextAccessor();

        var cs = config.GetConnectionString("OrdersDb")
                 ?? throw new InvalidOperationException("Connection string 'OrdersDb' is missing.");

        services.AddDbContext<OrdersDbContext>(opt =>
        {
            opt.UseSqlServer(cs);
        });

        // ----- Cross-cutting (required by OrdersDbContext ctor) -----
        services.AddScoped<ITenantProvider, HttpTenantProvider>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

        // ----- Persistence -----
        services.AddScoped<IOrderRepository, OrdersRepository>();
        services.AddScoped<ICustomerRepository, CustomersRepository>();
        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
        services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

        // ----- Read side -----
        services.AddScoped<IOrderReadQueries, EfOrderReadQueries>();

        return services;
    }
}
