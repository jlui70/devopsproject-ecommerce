using MediatR;
using DevOpsProjectEcommerce.Main.Domain.Commands;
using DevOpsProjectEcommerce.Main.Domain.Mappings;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.InOut.Responses;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Main.Domain.CommandHandlers
{
    public sealed class RegisterProductCommandHandler : IRequestHandler<RegisterProductCommand, ProductResponse>
    {
        private readonly ICreateEntityRepository<ProductEntity> _createEntityRepository;

        public RegisterProductCommandHandler
        (
            ICreateEntityRepository<ProductEntity> createEntityRepository
        )
        {
            _createEntityRepository = createEntityRepository ?? throw new ArgumentNullException(nameof(createEntityRepository));
        }

        public async Task<ProductResponse> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
        {
            var productEntity = await _createEntityRepository.ExecuteAsync(request.MapToEntity(), cancellationToken);
            return productEntity.MapToResponse();
        }
    }
}
