using TenantOrdersLab.App.Abstractions.Common;

namespace TenantOrdersLab.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tenant", (ITenantProvider tenantProvider) =>
        {
            return Results.Ok(new
            {
                tenantId = tenantProvider.TenantId
            });
        })
        .WithName("GetCurrentTenant")
        .WithTags("Tenant")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
