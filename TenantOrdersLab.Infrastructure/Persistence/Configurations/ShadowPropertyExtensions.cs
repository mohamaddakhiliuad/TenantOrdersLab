using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantOrdersLab.Domain.Abstractions;

namespace TenantOrdersLab.Infrastructure.Persistence.Configurations
{
    public static class ShadowPropertyExtensions
    {
        /// <summary>
        /// Adds tenant + audit + rowversion shadow properties for entities that implement
        /// ITenantScoped / IAudited.
        /// </summary>
        public static void AddInfrastructureShadowProperties<TEntity>(
            this EntityTypeBuilder<TEntity> builder)
            where TEntity : class
        {
            // TenantId (string, required)
            if (typeof(ITenantScoped).IsAssignableFrom(typeof(TEntity)))
            {
                builder.Property<string>("TenantId")
                       .IsRequired()
                       .HasMaxLength(64);

                builder.HasIndex("TenantId");
            }

            // Auditing timestamps (DateTime, required)
            if (typeof(IAudited).IsAssignableFrom(typeof(TEntity)))
            {
                builder.Property<DateTime>("CreatedAtUtc").IsRequired();
                builder.Property<DateTime>("UpdatedAtUtc").IsRequired();
            }

            // RowVersion (optimistic concurrency) — معمولاً برای همه مفید است
            builder.Property<byte[]>("RowVersion")
                   .IsRowVersion();
        }
    }
}
