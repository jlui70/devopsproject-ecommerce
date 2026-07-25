using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Main.Domain.Repositories.Contracts;

public interface IStockReadRepository: IReadEntityRepository<StockEntity>
{
    Task<StockEntity?> GetByProductIdAsync(int productId, CancellationToken cancellationToken);
}
