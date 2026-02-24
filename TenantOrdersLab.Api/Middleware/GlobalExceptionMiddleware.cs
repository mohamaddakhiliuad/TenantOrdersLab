using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.Api.Common;
using TenantOrdersLab.Domain.Common;

namespace TenantOrdersLab.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
///
/// Purpose:
/// - Catches unhandled exceptions from the entire request pipeline.
/// - Logs unexpected errors.
/// - Returns a consistent RFC 7807 ProblemDetails response.
/// - Adds a traceId for production debugging.
///
/// Why this is important:
/// - Prevents the API from crashing.
/// - Centralizes error handling (no try/catch in every endpoint).
/// - Provides clean, production-ready error responses.
/// </summary>
public sealed class GlobalExceptionMiddleware : IMiddleware
{
    // Logger used to record unexpected errors with trace information
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    // Logger is injected via Dependency Injection
    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        => _logger = logger;

    /// <summary>
    /// This method is executed for every HTTP request.
    ///
    /// Flow:
    /// 1. Pass the request to the next middleware/endpoint.
    /// 2. If everything succeeds → do nothing.
    /// 3. If an exception occurs → catch it here and return a controlled response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            // Continue the pipeline (next middleware, endpoint, etc.)
            await next(context);
        }

        // Special case:
        // If the client cancels the request (closed browser, network drop),
        // ASP.NET throws OperationCanceledException.
        // This is NOT a server error, so we avoid logging noise.
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {  // client aborted; don't log as error
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (DomainException dex)
        {
            // Business/domain failure -> map to the same contract as ResultHttpMapper.
            await ApiProblemFactory.WriteProblemAsync(dex.Message, context);
        }
        catch (DbUpdateConcurrencyException)
        {
            await ApiProblemFactory.WriteProblemAsync("conflict: The resource was modified by another request. Refresh and retry.", context);
           
        }
        // Any other unhandled exception from the application
        catch (Exception ex)
        {
            // Log the error with a unique TraceId for diagnostics
            _logger.LogError(
                ex,
                "Unhandled exception. TraceId={TraceId}",
                context.TraceIdentifier);

            // Create a standard RFC 7807 error response
            await ApiProblemFactory.WriteProblemAsync("Unexpected : An unexpected error occurred..", context);
           
              
        }
    }
}
