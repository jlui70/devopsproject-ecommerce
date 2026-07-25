using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.Main.Domain.Repositories.Contexts;
using DevOpsProjectEcommerce.Main.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Implementations;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Main.Domain.Repositories.Implementations;

public sealed class StockReadRepository: ReadEntityRepository<StockEntity>, IStockReadRepository
{
    private readonly ProductContext _context;
    public StockReadRepository(ProductContext context):
        base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<StockEntity?> GetByProductIdAsync(int productId, CancellationToken cancellationToken)
    {
        return await _context.Stock
            .AsNoTracking()
            .Include(stock => stock.Product)
            .FirstOrDefaultAsync(stock => stock.ProductId == productId, cancellationToken);
    }
}
