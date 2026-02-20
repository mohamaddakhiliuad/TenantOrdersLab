using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TenantOrdersLab.Api.Common;

/// <summary>
/// Single source of truth for converting application/domain error strings into RFC 7807 ProblemDetails.
///
/// Used by:
/// - ResultHttpMapper (controlled failures via Result)
/// - GlobalExceptionMiddleware (DomainException mapping)
///
/// Goal: eliminate duplication and keep API error responses consistent.
/// </summary>
public static class ApiProblemFactory
{
    public static IResult ToProblemResult(string? error, HttpContext http)
    {
        var problem = CreateProblem(error, http);
        return Results.Problem(
            detail: problem.Detail,
            instance: problem.Instance,
            statusCode: problem.Status,
            title: problem.Title,
            extensions: problem.Extensions);
    }

    public static async Task WriteProblemAsync(string? error, HttpContext http)
    {
        var problem = CreateProblem(error, http);

        http.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        http.Response.ContentType = MediaTypeNames.Application.Json;

        await http.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static ProblemDetails CreateProblem(string? error, HttpContext http)
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

        return problem;
    }
}