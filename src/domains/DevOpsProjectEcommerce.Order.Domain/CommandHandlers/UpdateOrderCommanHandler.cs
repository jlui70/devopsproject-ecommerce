using MediatR;
using DevOpsProjectEcommerce.Order.Domain.Commands;
using DevOpsProjectEcommerce.Order.Domain.Mappings;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.HttpHandlers.Contracts;
using DevOpsProjectEcommerce.Shared.InOut.Responses;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Order.Domain.CommandHandlers;

public sealed class UpdateOrderCommanHandler: IRequestHandler<UpdateOrderCommand, OrderResponse>
{
    private readonly IUpdateEntityRepository<OrderEntity> _updateEntityRepository;
    private readonly IMainApi _mainApiClient;
    private readonly IOrderReadRepository _readRepository;

    public UpdateOrderCommanHandler
    (
        IUpdateEntityRepository<OrderEntity> updateEntityRepository,
        IMainApi mainApiClient,
        IOrderReadRepository readRepository
    )
    {
        _updateEntityRepository = updateEntityRepository ?? throw new ArgumentNullException(nameof(updateEntityRepository));
        _mainApiClient = mainApiClient?? throw new ArgumentNullException(nameof(mainApiClient));
        _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
    }

    public async Task<OrderResponse> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var requestedOrderEntity = request.MapToEntity();
        var stockApiResponse = await _mainApiClient.GetStockByProductIdAsync(request.ProductId);
        if (!stockApiResponse.IsSuccessStatusCode)
            throw new KeyNotFoundException("Sorry, something went wrong while consulting the product stock.");

        var stockResponse = stockApiResponse.Content;
        if (stockResponse.Quantity < request.Quantity)
            throw new Exception("There is not enough stock for the selected Product.");
        
        var order =  await _readRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (order == null)
            throw new ArgumentNullException();

        order.Quantity = requestedOrderEntity.Quantity;
        
        await _updateEntityRepository.ExecuteAsync(order, cancellationToken);
        return order.MapToResponse();
    }
}
