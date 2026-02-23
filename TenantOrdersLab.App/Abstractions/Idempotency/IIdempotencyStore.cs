using TenantOrdersLab.App.Abstractions.Idempotency;

public interface IIdempotencyStore
{

     Task<IdempotencyDecision> TryBeginAsync(
        string tenantId,
        string key,
        byte[] requestHash,
        DateTime nowUtc,
        TimeSpan ttl,
        CancellationToken ct);

    Task CompleteAsync(string tenantId, string key, int orderId, CancellationToken ct);
}