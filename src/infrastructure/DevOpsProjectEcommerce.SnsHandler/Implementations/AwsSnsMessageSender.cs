using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using DevOpsProjectEcommerce.SnsHandler.Abstractions;
using DevOpsProjectEcommerce.SnsHandler.Exceptions;
using DevOpsProjectEcommerce.SnsHandler.Models;

namespace DevOpsProjectEcommerce.SnsHandler.Implementations;

public sealed class AwsSnsMessageSender: IMessageSender
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly AwsSnsMessageParams _snsMessageParams;

    public AwsSnsMessageSender(IAmazonSimpleNotificationService snsClient, AwsSnsMessageParams snsMessageParams)
    {
        _snsClient = snsClient ?? throw new ArgumentNullException(nameof(snsClient));
        _snsMessageParams = snsMessageParams?? throw new ArgumentNullException(nameof(snsMessageParams));
    }
    
    public async Task<string> EnqueueAsync<TObject>(TObject messageBody, CancellationToken cancellationToken)
    {
        var request = new PublishRequest
        {
            TopicArn = _snsMessageParams.TopicArn, 
            Message = JsonConvert.SerializeObject(messageBody)
        };

        var response = await _snsClient.PublishAsync(request, cancellationToken);
        if (string.IsNullOrEmpty(response.MessageId))
            throw new AwsSnsMessageSenderException("SNS did not return a MessageId — publish may have failed.");

        return response.MessageId;
    }
}
