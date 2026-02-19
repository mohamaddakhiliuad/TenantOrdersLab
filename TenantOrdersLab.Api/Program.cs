using TenantOrdersLab.Api.Endpoints;
using TenantOrdersLab.Api.Middleware;
using TenantOrdersLab.App.Order.Commands.CancelOrder;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TenantOrdersLab API", Version = "v1" });
});

// Cross-cutting for web
builder.Services.AddHttpContextAccessor();

// Infrastructure (DbContext + Repos + UoW + Tenant + Clock)
builder.Services.AddInfrastructure(builder.Configuration);

// Application
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<CancelOrderHandler>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapOrdersEndpoints();

app.Run();
