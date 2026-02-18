using Microsoft.AspNetCore.Mvc;
using TenantOrdersLab.App.Abstractions.Common;

namespace TenantOrdersLab.Api.Common;
/// <summary>
/// Converts Application-layer Result / Result&lt;T&gt; objects into HTTP responses in the API layer.
///
/// Purpose:
/// - Keep the Application layer independent from HTTP concerns (Clean Architecture).
/// - Centralize mapping of success/failure outcomes to consistent HTTP responses.
///
/// Behavior:
/// - Success (Result): returns 204 NoContent.
/// - Success (Result&lt;T&gt;): returns 200 OK with value, or allows a custom success response via onSuccess
///   (e.g., 201 Created for create endpoints).
/// - Failure: converts the error string into an RFC 7807 ProblemDetails response:
///   * Uses ApiErrorClassifier to detect error type (validation/not_found/conflict/unauthorized/forbidden)
///   * Maps to appropriate status code (400/404/409/401/403, otherwise 500)
///   * Adds traceId to ProblemDetails extensions for production diagnostics.
///
/// This mapper acts as an adapter between the Application layer and HTTP, ensuring consistent,
/// production-ready API responses without leaking web concerns into the core layers.
/// </summary>

public static class ResultHttpMapper
{
    public static IResult ToHttpResult(this Result result, HttpContext http)
    {
        if (result.IsSuccess) return Results.NoContent();

        return ProblemFrom(result.Error, http);
    }

    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext http, Func<T, IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            if (result.Value is null)
                return Results.Ok();

            return onSuccess is null ? Results.Ok(result.Value) : onSuccess(result.Value);
        }

        return ProblemFrom(result.Error, http);
    }

    private static IResult ProblemFrom(string? error, HttpContext http)
    {
        var (type, message) = ApiErrorClassifier.Classify(error);

        var (status, title) = type switch
        {
            ApiErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ApiErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ApiErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ApiErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ApiErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => (StatusCodes.Status500InternalServerError, "Request failed")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = message,
            Instance = http.Request.Path
        };

        problem.Extensions["traceId"] = http.TraceIdentifier;

        return Results.Problem(problem.Detail, problem.Instance, problem.Status, problem.Title, extensions: problem.Extensions);
    }
}
