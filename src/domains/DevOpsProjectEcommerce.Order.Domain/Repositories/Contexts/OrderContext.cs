using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Configurations;
using DevOpsProjectEcommerce.Shared.Enums;
using DevOpsProjectEcommerce.Shared.Models;
using DevOpsProjectEcommerce.Shared.Repositories.Configurations;

namespace DevOpsProjectEcommerce.Order.Domain.Repositories.Contexts
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }
        public DbSet<OrderEntity> Order { get; set; } = null!;
        public DbSet<ProductEntity> Product { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductEntityTypeConfiguration).Assembly);

            modelBuilder.Entity<StatusEntity>().HasData(
                new StatusEntity
                (
                    OrderStatus.Pendente,
                    Enum.GetName(OrderStatus.Pendente)!
                ),
                new StatusEntity
                (
                    OrderStatus.Confirmado,
                    Enum.GetName(OrderStatus.Confirmado)!
                )
            );
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
