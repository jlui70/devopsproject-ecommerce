using System.Net;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Refit;
using Xunit;
using DevOpsProjectEcommerce.Order.Domain.CommandHandlers;
using DevOpsProjectEcommerce.Order.Domain.Commands;
using DevOpsProjectEcommerce.Order.Domain.Events;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Shared.Enums;
using DevOpsProjectEcommerce.Shared.HttpHandlers.Contracts;
using DevOpsProjectEcommerce.Shared.InOut.Responses;
using DevOpsProjectEcommerce.Shared.Models;

namespace DevOpsProjectEcommerce.Order.Domain.Tests.CommandHandlers;

public class CreateOrderCommandHandlerTests
{
    private const string LoggedUserEmail = "buyer@devopsproject.com.br";

    private readonly Mock<ICreateEntityRepository<OrderEntity>> _createRepositoryMock = new();
    private readonly Mock<IMainApi> _mainApiClientMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IOrderReadRepository> _orderRepositoryMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();

    private CreateOrderCommandHandler CreateHandler() => new(
        _createRepositoryMock.Object,
        _mainApiClientMock.Object,
        _mediatorMock.Object,
        _httpContextAccessorMock.Object,
        _orderRepositoryMock.Object,
        _configurationMock.Object);

    private static CreateOrderCommand ValidCommand() => new()
    {
        ProductId = 42,
        Quantity = 2
    };

    private void SetupLoggedUser(string? email)
    {
        var claims = string.IsNullOrWhiteSpace(email)
            ? Array.Empty<Claim>()
            : new[] { new Claim(JwtRegisteredClaimNames.Email, email) };

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(context => context.User)
            .Returns(new ClaimsPrincipal(new ClaimsIdentity(claims)));

        _httpContextAccessorMock.SetupGet(accessor => accessor.HttpContext)
            .Returns(httpContextMock.Object);
    }

    private void SetupStock(bool isSuccessStatusCode, int quantityAvailable, decimal price = 100m)
    {
        var stockResponse = new StockResponse(
            Id: 1,
            Product: new ProductResponse(42, "Widget", price),
            Quantity: quantityAvailable);

        var httpResponseMessage = new HttpResponseMessage(
            isSuccessStatusCode ? HttpStatusCode.OK : HttpStatusCode.NotFound);

        var apiResponse = new ApiResponse<StockResponse>(
            httpResponseMessage,
            stockResponse,
            new RefitSettings());

        _mainApiClientMock
            .Setup(api => api.GetStockByProductIdAsync(It.IsAny<int>()))
            .ReturnsAsync(apiResponse);
    }

    private void SetupLocalStack(bool isEnabled)
    {
        _configurationMock
            .Setup(config => config["LocalStack:IsEnabled"])
            .Returns(isEnabled ? "true" : "false");
    }

    [Fact]
    public async Task Handle_ValidOrderWithSufficientStock_CreatesOrderAndReturnsResponse()
    {
        // Arrange
        SetupLoggedUser(LoggedUserEmail);
        SetupStock(isSuccessStatusCode: true, quantityAvailable: 10, price: 50m);
        SetupLocalStack(isEnabled: false);

        OrderEntity? capturedEntity = null;
        _createRepositoryMock
            .Setup(repo => repo.ExecuteAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Callback<OrderEntity, CancellationToken>((entity, _) => capturedEntity = entity)
            .ReturnsAsync((OrderEntity entity, CancellationToken _) => entity);

        var expectedResponseEntity = new OrderEntity(42, 2, LoggedUserEmail, OrderStatus.Pendente)
        {
            Id = 7,
            Product = new ProductEntity { Id = 42, Name = "Widget", Price = 50m }
        };
        _orderRepositoryMock
            .Setup(repo => repo.GetByProductIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponseEntity);

        var handler = CreateHandler();

        // Act
        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(7, response.Id);
        Assert.Equal(2, response.Quantity);
        Assert.Equal(LoggedUserEmail, response.BoughtBy);
        _createRepositoryMock.Verify(
            repo => repo.ExecuteAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal(LoggedUserEmail, capturedEntity!.BoughtBy);
    }

    [Fact]
    public async Task Handle_ValidOrderOutsideLocalStack_PersistsOrderWithPendenteStatus()
    {
        // Arrange: business rule - orders confirmed via Lambda (AWS path) must start as Pendente,
        // not Confirmado, regardless of the default set by the command-to-entity mapping.
        SetupLoggedUser(LoggedUserEmail);
        SetupStock(isSuccessStatusCode: true, quantityAvailable: 5);
        SetupLocalStack(isEnabled: false);

        OrderEntity? capturedEntity = null;
        _createRepositoryMock
            .Setup(repo => repo.ExecuteAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Callback<OrderEntity, CancellationToken>((entity, _) => capturedEntity = entity)
            .ReturnsAsync((OrderEntity entity, CancellationToken _) => entity);

        _orderRepositoryMock
            .Setup(repo => repo.GetByProductIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderEntity(42, 2, LoggedUserEmail, OrderStatus.Pendente)
            {
                Id = 1,
                Product = new ProductEntity { Id = 42, Name = "Widget", Price = 10m }
            });

        var handler = CreateHandler();

        // Act
        await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(OrderStatus.Pendente, capturedEntity!.StatusId);
    }

    [Fact]
    public async Task Handle_QuantityExceedsAvailableStock_ThrowsException()
    {
        // Arrange: business rule - can't sell more than what's in stock.
        SetupLoggedUser(LoggedUserEmail);
        SetupStock(isSuccessStatusCode: true, quantityAvailable: 1);
        SetupLocalStack(isEnabled: false);

        var handler = CreateHandler();
        var command = ValidCommand(); // Quantity = 2, stock = 1

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(command, CancellationToken.None));
        Assert.Equal("There is not enough stock for the selected Product.", exception.Message);
        _createRepositoryMock.Verify(
            repo => repo.ExecuteAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_StockApiCallFails_ThrowsKeyNotFoundException()
    {
        // Arrange
        SetupLoggedUser(LoggedUserEmail);
        SetupStock(isSuccessStatusCode: false, quantityAvailable: 10);
        SetupLocalStack(isEnabled: false);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(ValidCommand(), CancellationToken.None));
        _createRepositoryMock.Verify(
            repo => repo.ExecuteAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NoAuthenticatedUserEmailClaim_ThrowsArgumentNullException()
    {
        // Arrange: business rule - an order must always be attributable to a logged-in buyer.
        SetupLoggedUser(email: null);

        var handler = CreateHandler();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.Handle(ValidCommand(), CancellationToken.None));
        _mainApiClientMock.Verify(api => api.GetStockByProductIdAsync(It.IsAny<int>()), Times.Never);
    }
}
