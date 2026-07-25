using Newtonsoft.Json;
using DevOpsProjectEcommerce.SesHandler.Abstractions;
using DevOpsProjectEcommerce.SesHandler.Models;
using DevOpsProjectEcommerce.SqsHandler.Abstractions;
using DevOpsProjectEcommerce.SqsHandler.Models;

namespace DevOpsProjectEcommerce.Notificator.Tasks
{
    public sealed class AwsSesEmailSenderProcessor : IMessageProcessor
    {
        private readonly IEmailSender _emailSender;
        public AwsSesEmailSenderProcessor
        (
            IEmailSender emailSender
        )
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        }

        public async Task ProcessMessageAsync(AwsQueueMessageParams awsQueueMessage, CancellationToken cancellationToken)
        {
            if (awsQueueMessage is null)
                throw new ArgumentNullException(nameof(awsQueueMessage));
            
            var emailParams = JsonConvert.DeserializeObject<EmailParams>(awsQueueMessage.Body);
            if (emailParams is null)
                throw new ArgumentNullException(nameof(emailParams));
            
            await _emailSender.SendAsync(emailParams, cancellationToken);
        }
    }
}
