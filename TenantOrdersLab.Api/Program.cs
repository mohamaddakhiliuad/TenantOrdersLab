using FluentValidation;
using TenantOrdersLab.Api.DependencyInjection;
using TenantOrdersLab.Api.Endpoints;
using TenantOrdersLab.Api.Middleware;
using TenantOrdersLab.Api.Validators.Orders;
using TenantOrdersLab.App.DependencyInjection;

using TenantOrdersLab.Infrastructure.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);
// Swagger
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TenantOrdersLab API", Version = "v1" });
});

// Cross-cutting for web
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddApi();

// Infrastructure (DbContext + Repos + UoW + Tenant + Clock)
builder.Services.AddInfrastructure(builder.Configuration);
// Application
builder.Services.AddApplication();


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapOrdersEndpoints();
app.MapOrdersReadEndpoints();
app.MapTenantEndpoints();
app.Run();
