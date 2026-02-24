namespace TenantOrdersLab.Api.Common;

public enum ApiErrorType
{
    unexpected,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Failure
}

/// <summary>
/// Classifies raw error strings returned from the Application layer into structured API error information.
///
/// The Application layer remains independent of HTTP and returns simple error messages
/// using lightweight prefixes such as:
/// 
/// validation:
/// not_found:
/// conflict:
/// unauthorized:
/// forbidden:
///
/// This classifier:
/// 1. Detects the error type based on the prefix (case-insensitive)
/// 2. Extracts a clean, user-friendly message
/// 3. Returns a tuple containing ApiErrorType and the message
///
/// The result is later used by the API layer to map errors to appropriate HTTP status codes
/// and ProblemDetails responses.
///
/// Design goals:
/// - Preserve Clean Architecture boundaries (no HTTP dependency in Application)
/// - Centralize error classification logic
/// - Provide a lightweight alternative to exception-based control flow
/// - Ensure consistent and production-ready error handling
/// </summary>

public static class ApiErrorClassifier
{
    public static (ApiErrorType type, string message) Classify(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return (ApiErrorType.Failure, "Unknown error.");

        var e = error.Trim();
        if (e.StartsWith("Unexpected:", StringComparison.OrdinalIgnoreCase))
            return (ApiErrorType.Validation, e["Unexpected:".Length..].Trim());

        if (e.StartsWith("validation:", StringComparison.OrdinalIgnoreCase))
            return (ApiErrorType.Validation, e["validation:".Length..].Trim());

        if (e.StartsWith("not_found:", StringComparison.OrdinalIgnoreCase))
            return (ApiErrorType.NotFound, e["not_found:".Length..].Trim());

        if (e.StartsWith("conflict:", StringComparison.OrdinalIgnoreCase))
            return (ApiErrorType.Conflict, e["conflict:".Length..].Trim());

        if (e.StartsWith("unauthorized:", StringComparison.OrdinalIgnoreCase))
            return (ApiErrorType.Unauthorized, e["unauthorized:".Length..].Trim());

        if (e.StartsWith("forbidden:", StringComparison.OrdinalIgnoreCase))
            return (ApiErrorType.Forbidden, e["forbidden:".Length..].Trim());

        return (ApiErrorType.Failure, e);
    }
}
