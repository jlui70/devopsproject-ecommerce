using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contexts;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Implementations;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Order.Domain.Repositories.Implementations;

public sealed class OrderReadRepository : ReadEntityRepository<OrderEntity>, IOrderReadRepository
{
    private readonly OrderContext _context;

    public OrderReadRepository(OrderContext context) :
        base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OrderEntity?> GetByProductIdAsync(int productId, CancellationToken cancellationToken)
    {
        return await _context.Order
            .AsNoTracking()
            .Include(order => order.Product)
            .FirstOrDefaultAsync(order => order.ProductId ==productId, cancellationToken: cancellationToken);
    }
}
