using Amazon.Lambda;
using Amazon.Lambda.Model;
using Autofac;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DevOpsProjectEcommerce.Order.Domain.Events;
using DevOpsProjectEcommerce.Shared.Events;
using ISnsMessageSender = DevOpsProjectEcommerce.SnsHandler.Abstractions.IMessageSender;

namespace DevOpsProjectEcommerce.Order.Domain.EventHandlers;

public sealed class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    private readonly IComponentContext _context;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, IComponentContext context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        var configuration = _context.Resolve<IConfiguration>();
        var isLocalStack  = Convert.ToBoolean(configuration["LocalStack:IsEnabled"]);

        if (isLocalStack)
        {
            // LocalStack: publica direto no SNS (fluxo local aprovado pelo board)
            _logger.LogInformation("Order {OrderId} — LocalStack path: publishing to SNS directly", @event.Id);
            var snsSender = _context.ResolveNamed<ISnsMessageSender>("SnsOrderConfirmedSender");
            await snsSender.EnqueueAsync(new OrderConfirmedEvent { Id = @event.Id }, cancellationToken);
            _logger.LogInformation("OrderConfirmedEvent published to SNS for OrderId={OrderId}", @event.Id);
            return;
        }

        // AWS path: invoca Lambda (simula gateway de pagamento) — fire and forget
        _logger.LogInformation("Order {OrderId} — AWS path: invoking Lambda orderConfirmedLambdaFunction", @event.Id);

        var lambdaClient  = _context.Resolve<IAmazonLambda>();
        var functionName  = configuration["Order:LambdaParams:FunctionName"] ?? "orderConfirmedLambdaFunction";

        var payload = JsonConvert.SerializeObject(new
        {
            orderId       = @event.Id,
            customerId    = @event.BoughtBy,
            totalAmount   = @event.TotalAmount,
            customerEmail = @event.BoughtBy,
            items         = new[] { new { productId = @event.ProductId, quantity = @event.Quantity } }
        });

        await lambdaClient.InvokeAsync(new InvokeRequest
        {
            FunctionName   = functionName,
            InvocationType = InvocationType.Event, // async — não bloqueia o pedido
            Payload        = payload
        }, cancellationToken);

        _logger.LogInformation("Lambda {Function} invoked async for OrderId={OrderId}", functionName, @event.Id);
    }
}
