using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using DevOpsProjectEcommerce.Order.Domain.Commands;
using DevOpsProjectEcommerce.Order.Domain.Events;
using DevOpsProjectEcommerce.Order.Domain.Mappings;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.Enums;
using DevOpsProjectEcommerce.Shared.HttpHandlers.Contracts;
using DevOpsProjectEcommerce.Shared.InOut.Responses;
using DevOpsProjectEcommerce.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace DevOpsProjectEcommerce.Order.Domain.CommandHandlers;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IMainApi _mainApiClient;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICreateEntityRepository<OrderEntity> _createRepository;
    private readonly IOrderReadRepository _orderRepository;
    private readonly IConfiguration _configuration;

    public CreateOrderCommandHandler
    (
        ICreateEntityRepository<OrderEntity> repository,
        IMainApi mainApiClient,
        IMediator mediator,
        IHttpContextAccessor httpContextAccessor,
        IOrderReadRepository orderRepository,
        IConfiguration configuration
    )
    {
        _createRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mainApiClient = mainApiClient ?? throw new ArgumentNullException(nameof(mainApiClient));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var loggedUser = _httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(claim => claim.Type.Contains(JwtRegisteredClaimNames.Email))
            ?.Value;
        if (string.IsNullOrWhiteSpace(loggedUser))
            throw new ArgumentNullException();

        var stockApiResponse = await _mainApiClient.GetStockByProductIdAsync(request.ProductId);
        if (!stockApiResponse.IsSuccessStatusCode)
            throw new KeyNotFoundException("Sorry, something went wrong while consulting the product stock.");

        var stockResponse = stockApiResponse.Content;
        if (stockResponse.Quantity < request.Quantity)
            throw new Exception("There is not enough stock for the selected Product.");

        var requestedOrderEntity = request.WithBoughBy(loggedUser).MapToEntity();

        // AWS path: Lambda confirms the order → starts as Pendente
        // LocalStack path: SNS direct confirmation → starts as Confirmado
        var isLocalStack = Convert.ToBoolean(_configuration["LocalStack:IsEnabled"]);
        if (!isLocalStack)
            requestedOrderEntity.StatusId = OrderStatus.Pendente;

        await _createRepository.ExecuteAsync(requestedOrderEntity, cancellationToken);

        var totalAmount = (decimal)(stockResponse.Product?.Price ?? 0) * request.Quantity;
        await _mediator.Publish(new OrderCreatedEvent(request)
        {
            Id          = requestedOrderEntity.Id,
            TotalAmount = totalAmount
        }, cancellationToken);

        var createdOrderEntity = await _orderRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (createdOrderEntity == null)
            throw new ArgumentNullException();

        return createdOrderEntity.MapToResponse();
    }
}
