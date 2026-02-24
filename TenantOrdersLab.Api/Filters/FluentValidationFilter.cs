using FluentValidation;
using Microsoft.AspNetCore.Http;
using TenantOrdersLab.Api.Common;

namespace TenantOrdersLab.Api.Filters;

/// <summary>
/// Minimal API EndpointFilter that validates the first argument of type TRequest
/// using FluentValidation. If invalid, returns 400 ValidationProblemDetails
/// (consistent with ApiProblemFactory contract).
/// </summary>
public sealed class FluentValidationFilter<TRequest> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find the request object among endpoint args
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
            return await next(context);

        var http = context.HttpContext;
        var validator = http.RequestServices.GetService<IValidator<TRequest>>();

        // If no validator registered, continue (safe default)
        if (validator is null)
            return await next(context);

        var ct = http.RequestAborted;
        var result = await validator.ValidateAsync(request, ct);

        if (result.IsValid)
            return await next(context);

        // Build validation payload consistent with your API contract
        var errors = result.ToProblemDetails();
        await ApiProblemFactory.WriteValidationProblemAsync(errors, http);

        // We already wrote the response body
        return Results.Empty;
    }
}