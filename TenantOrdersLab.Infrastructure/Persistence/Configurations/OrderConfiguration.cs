using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantOrdersLab.Domain;

namespace TenantOrdersLab.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            // Key
            builder.HasKey(x => x.Id);

            // چون Id رو خودت می‌دی (در ctor می‌سازی)، معمولاً اینو می‌ذاریم:
            builder.Property(x => x.Id)
                   .ValueGeneratedNever();

            // Simple properties
            builder.Property(x => x.CustomerId)
                   .IsRequired();
            // FK relationship: Order.CustomerId -> Customer.Id
            builder.HasOne(o => o.Customer)
                   .WithMany()                  // Customer collection نداریم (می‌تونی بعداً اضافه کنی)
                   .HasForeignKey(o => o.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict); // حذف Customer باعث حذف Order نشه

            builder.HasIndex(o => o.CustomerId);


            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<int>(); // enum as int (default هم هست، ولی explicit بهتره)

            // Money as Value Object (Owned)
            builder.OwnsOne(x => x.Total, money =>
            {
                money.Property(p => p.Amount)
                     .HasColumnName("TotalAmount")
                     .HasPrecision(18, 2)
                     .IsRequired();

                money.Property(p => p.Currency)
                     .HasColumnName("TotalCurrency")
                     .HasMaxLength(3)
                     .IsRequired();
            });
            builder.Navigation(x => x.Total).IsRequired();

            // -------------------------------------------------
            // Step 3.3 — Shadow Properties (Infrastructure Policy)
            // -------------------------------------------------
            builder.Property<DateTime?>("PlacedAtUtc");


            builder.AddInfrastructureShadowProperties();

            // Domain events should NOT be persisted
            builder.Ignore("DomainEvents");  // اگر property public داشتی
            builder.Ignore("_domainEvents"); // backing field private
        }
    }
}
