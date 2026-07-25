using MediatR;
using DevOpsProjectEcommerce.Main.Domain.Commands;
using DevOpsProjectEcommerce.Main.Domain.Mappings;
using DevOpsProjectEcommerce.Main.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.InOut.Responses;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Main.Domain.CommandHandlers
{
    public sealed class RegisterProductStockCommandHandler : IRequestHandler<RegisterProductStockCommand, StockResponse>
    {
        private readonly ICreateEntityRepository<StockEntity> _createEntityRepository;
        private readonly IStockReadRepository _stockReadRepository;

        public RegisterProductStockCommandHandler
        (
            ICreateEntityRepository<StockEntity> createEntityRepository,
            IStockReadRepository stockReadRepository
        )
        {
            _createEntityRepository = createEntityRepository ?? throw new ArgumentNullException(nameof(createEntityRepository));
            _stockReadRepository = stockReadRepository ?? throw new ArgumentNullException(nameof(stockReadRepository));
        }

        public async Task<StockResponse> Handle(RegisterProductStockCommand request, CancellationToken cancellationToken)
        {
            var oldStock = await _stockReadRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (oldStock is not null)
                throw new ArgumentException("The specified product already has a stock registered, please use" +
                                               "the UPDATE method.");
            
            var stockToCreate = request.MapToEntity();
            await _createEntityRepository.ExecuteAsync(stockToCreate, cancellationToken);
            
            var createdStock = await _stockReadRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (createdStock == null)
                throw new ArgumentNullException();
            
            return createdStock.MapToResponse();
        }
    }
}
