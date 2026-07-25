using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Order.Domain.Repositories.Contracts;

public interface IOrderReadRepository: IReadEntityRepository<OrderEntity>
{
    Task<OrderEntity?> GetByProductIdAsync(int productId, CancellationToken cancellationToken);
}
