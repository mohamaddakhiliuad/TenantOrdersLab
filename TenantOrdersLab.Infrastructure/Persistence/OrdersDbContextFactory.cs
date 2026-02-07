/*using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TenantOrdersLab.Infrastructure.Persistence
{
    public sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
    {
        public OrdersDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TenantOrdersLab;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;

            //return new OrdersDbContext(options);
        }
    }
}
*/