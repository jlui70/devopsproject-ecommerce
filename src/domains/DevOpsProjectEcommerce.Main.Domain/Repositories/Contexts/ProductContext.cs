using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.Main.Domain.Repositories.Configurations;
using DevOpsProjectEcommerce.Shared.Models;
using DevOpsProjectEcommerce.Shared.Repositories.Configurations;

namespace DevOpsProjectEcommerce.Main.Domain.Repositories.Contexts
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options): base(options) { }

        public DbSet<StockEntity> Stock { get; set; } = null!;
        public DbSet<ProductEntity> Product { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockEntityTypeConfiguration).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
