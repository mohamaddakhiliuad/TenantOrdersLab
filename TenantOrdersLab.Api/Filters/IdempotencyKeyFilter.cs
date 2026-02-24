using TenantOrdersLab.Api.Common;

namespace TenantOrdersLab.Api.Filters;

public sealed class IdempotencyKeyFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;

        var idemKey = http.Request.Headers["Idempotency-Key"].ToString();
        if (string.IsNullOrWhiteSpace(idemKey))
        {
            // Consistent with your Result error contract (400)
            await ApiProblemFactory.WriteProblemAsync("validation: Idempotency-Key header is required.", http);
            return Results.Empty;
        }

        return await next(context);
    }
}