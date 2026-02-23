using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantOrdersLab.Infrastructure.Persistence.Idempotency;

namespace TenantOrdersLab.Infrastructure.Persistence.Configurations
{
    public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("IdempotencyRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Key).HasMaxLength(128).IsRequired();
            builder.Property(x => x.RequestHash).IsRequired();

            builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            builder.HasIndex(x => x.ExpiresAtUtc);

            // optional: if you want consistent tinyint mapping
            builder.Property(x => x.Status).HasConversion<byte>();
        }
    }
}
