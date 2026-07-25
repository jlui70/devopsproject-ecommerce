using DevOpsProjectEcommerce.SqsHandler.Models;

namespace DevOpsProjectEcommerce.SqsHandler.Abstractions
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(AwsQueueMessageParams awsQueueMessage, CancellationToken cancellationToken);
    }
}
