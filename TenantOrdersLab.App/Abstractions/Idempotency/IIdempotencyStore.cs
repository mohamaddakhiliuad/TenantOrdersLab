// -----------------------------------------------------------------------------
// IIdempotencyStore
// -----------------------------------------------------------------------------
// Architectural Boundary for Idempotent Write Coordination.
//
// This abstraction allows the Application layer to coordinate
// safe, idempotent POST operations without knowing how persistence works.
//
// Responsibilities:
//
// - Detect duplicate logical requests (same tenant + key)
// - Prevent concurrent race conditions
// - Enforce payload consistency via request hash comparison
// - Manage TTL (expiration) for idempotency records
//
// Important:
// • This interface lives in Application.
// • Implementation lives in Infrastructure.
// • The Domain layer remains completely unaware of idempotency.
// • HTTP concerns (headers) are resolved before reaching this boundary.
//
// In short:
// Application decides.
// Infrastructure guarantees.
// Database enforces.
// -----------------------------------------------------------------------------

using TenantOrdersLab.App.Abstractions.Idempotency;

public interface IIdempotencyStore
{
    /// <summary>
    /// Attempts to begin an idempotent operation.
    /// Returns a decision that indicates:
    /// - New request (safe to proceed)
    /// - Duplicate (return previous result)
    /// - Conflict (same key, different payload)
    /// - InProgress (concurrent execution)
    /// </summary>
    Task<IdempotencyDecision> TryBeginAsync(  string tenntId,string key,byte[] requestHash,
        DateTime nowUtc,TimeSpan ttl,CancellationToken ct);

    /// <summary>
    /// Marks the idempotent operation as completed
    /// and associates the resulting OrderId.
    /// </summary>
    Task CompleteAsync(string tenantId,string key,int orderId,CancellationToken ct);
}