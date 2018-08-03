using eShopDashboard.Infrastructure.Data.Ordering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scripts.Cli
{
    public class OrderingContextDesignTimeFactory : IDesignTimeDbContextFactory<OrderingContext>
    {
        public OrderingContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<OrderingContext>();

            builder.UseSqlServer("x");

            return new OrderingContext(builder.Options);
        }
    }
}