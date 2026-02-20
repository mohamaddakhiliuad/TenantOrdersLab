# TenantOrdersLab – Logging & Error Handling Architecture

## Overview

This document describes the complete logging and error handling strategy used in the TenantOrdersLab project.

The goal of this design is to:

- Separate business failures from system failures
- Preserve Clean Architecture boundaries
- Centralize HTTP error shaping
- Provide production-ready structured responses
- Ensure traceability through consistent logging

---

# 1. Architectural Principles

The system follows four core principles:

1. Domain rules throw exceptions (invariants enforcement)
2. Application flow failures return Result objects
3. HTTP error shaping happens only in the API layer
4. Unexpected failures are handled globally via middleware

This prevents HTTP concerns from leaking into Domain or Application layers.

---

# 2. Error Categories

The system distinguishes between two main categories of errors.

## 2.1 Business / Expected Failures

Examples:

- Validation errors
- Not found errors
- Conflict errors
- Unauthorized / Forbidden

These are predictable outcomes of normal business logic.

They are represented as:

- Result.Failure(AppError)
- or DomainException (for invariant violations)

These are mapped to proper HTTP status codes (400–409 range).

---

## 2.2 System / Unexpected Failures

Examples:

- NullReferenceException
- SQL exceptions
- Infrastructure crashes
- Programming bugs

These are not part of normal business flow.

They are:

- Logged
- Converted to 500 Internal Server Error
- Returned as safe ProblemDetails responses

---

# 3. Result Pattern (Application Layer)

Application use cases return a minimal Result / Result<T> type.

Purpose:

- Make workflow explicit
- Avoid using exceptions for normal control flow
- Keep handlers interview-friendly and readable

Example:

Success:

Result<T>.Success(value)

Failure:

Result<T>.Failure(new AppError(ErrorType.NotFound, "Customer not found."))

This keeps business flow predictable and testable.

---

# 4. Structured Error Model (AppError)

Instead of returning raw strings, errors are represented as a structured object:

AppError:

- Type (Validation, NotFound, Conflict, etc.)
- Message (summary)
- Optional field-level errors dictionary

This enables advanced validation responses such as:

{
  "errors": {
    "TotalAmount": ["Must be greater than zero"],
    "Currency": ["Invalid currency"]
  }
}

This structure eliminates fragile string prefix parsing and improves maintainability.

---

# 5. API Layer – ResultHttpMapper

ResultHttpMapper converts Result objects into HTTP responses.

Responsibilities:

- Map Success → 200 / 201 / 204
- Map Failure → ProblemDetails via ApiProblemFactory
- Keep endpoints clean (single-line return)

Example usage inside endpoint:

return result.ToHttpResult(http);

This prevents duplication of error mapping logic across endpoints.

---

# 6. ApiProblemFactory – Single Source of Truth

ApiProblemFactory centralizes the creation of ProblemDetails responses.

Responsibilities:

- Map ErrorType → HTTP status code
- Attach traceId
- Generate consistent RFC 7807 responses
- Eliminate duplicated mapping logic

This ensures:

- Uniform API contract
- Maintainable error shaping
- Clear separation of concerns

---

# 7. GlobalExceptionMiddleware

GlobalExceptionMiddleware is the final safety net in the HTTP pipeline.

It handles:

1. DomainException → mapped to business error responses
2. OperationCanceledException → handled without noisy logging
3. Unexpected Exception → logged and converted to 500

Responsibilities:

- Prevent API crashes
- Log unhandled exceptions
- Return consistent ProblemDetails responses
- Attach traceId for diagnostics

This middleware ensures system stability under failure conditions.

---

# 8. Logging Strategy

Logging is centralized within GlobalExceptionMiddleware.

For unexpected exceptions:

- The exception is logged with ILogger
- The TraceIdentifier is included
- A safe 500 response is returned

Example log pattern:

"Unhandled exception. TraceId={TraceId}"

TraceId allows correlation between:

- API response
- Server logs
- Observability tools

This design supports production debugging and distributed tracing.

---

# 9. End-to-End Failure Flow

Controlled Failure:

Domain/Application → Result or DomainException → API Mapping → ProblemDetails (4xx)

Unexpected Failure:

Exception → GlobalExceptionMiddleware → Log + 500 ProblemDetails

This separation ensures clarity, maintainability, and scalability.

---

# 10. Production Readiness Benefits

This architecture provides:

- Clean separation between layers
- No HTTP dependencies in Domain
- No duplicated error mapping logic
- Centralized error shaping
- Traceable logging
- Interview-ready architectural clarity

---

# Final Statement

The TenantOrdersLab error handling strategy follows this principle:

Business failures are explicit.
System failures are isolated.
HTTP shaping is centralized.
Logging is structured and traceable.

This design aligns with Clean Architecture and production-grade backend standards.

