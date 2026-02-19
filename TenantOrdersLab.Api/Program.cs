using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.Api.Middleware;
using TenantOrdersLab.Api.Endpoints;
using TenantOrdersLab.Infrastructure.Persistence;
using TenantOrdersLab.Infrastructure.Persistence.Queries;
using TenantOrdersLab.App.Abstractions.Persistence;
using TenantOrdersLab.App.Order.Commands.CreateOrder;
using TenantOrdersLab.App.Order.Commands.CancelOrder;
using TenantOrdersLab.Infrastructure.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// ---------- Swagger (Swashbuckle) ----------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TenantOrdersLab API", Version = "v1" });
});

// ---------- Middleware ----------
//builder.Services.AddTransient<GlobalExceptionMiddleware>();

// ---------- DbContext ----------

var cs = builder.Configuration.GetConnectionString("OrdersDb")
         ?? throw new InvalidOperationException("Connection string 'OrdersDb' is missing.");

builder.Services.AddDbContext<OrdersDbContext>(opt =>
{
    opt.UseSqlServer(cs);
   
});

// ---------- Infra: Repos + UoW + Queries ----------
// ⚠️ اینجا فرض می‌کنم شما Implementation ها رو داری.
// اگر اسم کلاس‌ها متفاوت بود، فقط اسم‌ها رو جایگزین کن.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IOrderRepository, OrdersRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomersRepository >();
builder.Services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();

builder.Services.AddScoped<IOrderReadQueries, EfOrderReadQueries>();

// ---------- Application Handlers ----------
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

// API routes
app.MapOrdersEndpoints();

app.Run();
