using Microsoft.EntityFrameworkCore;
using TenantOrdersLab.App.Abstractions.Idempotency;

//using TenantOrdersLab.App.Abstractions.Idempotency;
using TenantOrdersLab.Infrastructure.Persistence;

namespace TenantOrdersLab.Infrastructure.Persistence.Idempotency;

internal sealed class EfIdempotencyStore : IIdempotencyStore
{
    private readonly OrdersDbContext _db;

    public EfIdempotencyStore(OrdersDbContext db) => _db = db;

    public async Task<IdempotencyDecision> TryBeginAsync(
        string tenantId,
        string key,
        byte[] requestHash,
        DateTime nowUtc,
        TimeSpan ttl,
        CancellationToken ct)
    {
        // 1) Check existing record (same tenant+key)
        var existing = await _db.IdempotencyRecords
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key, ct);

        if (existing is not null)
        {
            // payload mismatch => key reuse conflict
            if (!existing.RequestHash.SequenceEqual(requestHash))
                return new IdempotencyDecision(IsDuplicate: false, IsInProgress: false, HasConflict: true, ExistingOrderId: null);

            // completed => duplicate (return same result)
            if (existing.Status == (byte)IdempotencyStatus.Completed && existing.OrderId.HasValue)
                return new IdempotencyDecision(IsDuplicate: true, IsInProgress: false, HasConflict: false, ExistingOrderId: existing.OrderId);

            // still processing
            return new IdempotencyDecision(IsDuplicate: false, IsInProgress: true, HasConflict: false, ExistingOrderId: null);
        }

        // 2) Create InProgress record (unique index enforces concurrency safety)
        var rec = new IdempotencyRecord
        {
            TenantId = tenantId,
            Key = key,
            RequestHash = requestHash,
            Status = (byte)IdempotencyStatus.InProgress,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(ttl)
        };

        _db.IdempotencyRecords.Add(rec);

        try
        {
            await _db.SaveChangesAsync(ct);
            return new IdempotencyDecision(IsDuplicate: false, IsInProgress: false, HasConflict: false, ExistingOrderId: null);
        }
        catch (DbUpdateException)
        {
            // Race: another request inserted same (TenantId,Key) between our read and insert.
            // Re-read and decide.
            var raced = await _db.IdempotencyRecords
                .SingleAsync(x => x.TenantId == tenantId && x.Key == key, ct);

            if (!raced.RequestHash.SequenceEqual(requestHash))
                return new IdempotencyDecision(false, false, true, null);

            if (raced.Status == (byte)IdempotencyStatus.Completed && raced.OrderId.HasValue)
                return new IdempotencyDecision(true, false, false, raced.OrderId);

            return new IdempotencyDecision(false, true, false, null);
        }
    }

    public async Task CompleteAsync(string tenantId, string key, int orderId, CancellationToken ct)
    {
        var rec = await _db.IdempotencyRecords
            .SingleAsync(x => x.TenantId == tenantId && x.Key == key, ct);

        rec.OrderId = orderId;
        rec.Status = (byte)IdempotencyStatus.Completed;

        await _db.SaveChangesAsync(ct);
    }

    public Task<int> CleanupExpiredAsync(DateTime nowUtc, CancellationToken ct)
        => _db.IdempotencyRecords
            .Where(x => x.ExpiresAtUtc <= nowUtc)
            .ExecuteDeleteAsync(ct);

   
   
}