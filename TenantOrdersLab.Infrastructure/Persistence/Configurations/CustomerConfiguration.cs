using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantOrdersLab.Domain;

namespace TenantOrdersLab.Infrastructure.Persistence.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>

    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            // Key
            builder.HasKey(x => x.Id);
            // چون Id رو خودت می‌دی (در ctor می‌سازی)، معمولاً اینو می‌ذاریم:
            builder.Property(x => x.Id)
                   .ValueGeneratedNever();
            // Simple properties
            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);
            // -------------------------------------------------
            // Step 3.3 — Shadow Properties (Infrastructure Policy)
            // -------------------------------------------------
            builder.AddInfrastructureShadowProperties();
        }

        
       
    }
}
