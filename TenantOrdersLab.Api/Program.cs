using Microsoft.OpenApi;

using TenantOrdersLab.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TenantOrdersLab API",
        Version = "v1"
    });

    // Optional: show XML comments in Swagger (if you enable XML docs in csproj)
    // var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // opt.IncludeXmlComments(xmlPath);
});

builder.Services.AddHealthChecks();
builder.Services.AddTransient<GlobalExceptionMiddleware>();

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TenantOrdersLab API v1");
        c.RoutePrefix = "swagger"; // default anyway
    });
}

app.MapHealthChecks("/health");

// Group for all API endpoints
var api = app.MapGroup("/api");

// TODO: map endpoints here, e.g. api.MapPost("/orders", ...);

app.Run();
