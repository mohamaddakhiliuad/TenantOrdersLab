namespace TenantOrdersLab.Infrastructure.Persistence.Idempotency;

public sealed class IdempotencyRecord
{
    public long Id { get; set; }

    public string TenantId { get; set; } = default!;
    public string Key { get; set; } = default!;
    public byte[] RequestHash { get; set; } = default!; // SHA-256 (32 bytes)

    public int? OrderId { get; set; }
    public byte Status { get; set; } // 0 InProgress, 1 Completed

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}