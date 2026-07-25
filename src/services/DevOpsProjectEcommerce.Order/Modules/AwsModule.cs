using Amazon.Lambda;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Options;
using DevOpsProjectEcommerce.SnsHandler.Implementations;
using DevOpsProjectEcommerce.SnsHandler.Models;
using DevOpsProjectEcommerce.SqsHandler.Abstractions;
using DevOpsProjectEcommerce.SqsHandler.Implementations;
using DevOpsProjectEcommerce.SqsHandler.Models;

namespace DevOpsProjectEcommerce.Order.Modules
{
    public class AwsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // SQS client (LocalStack-aware)
            builder.Register(componentContext =>
            {
                var configuration = componentContext.Resolve<IConfiguration>();
                if (Convert.ToBoolean(configuration["LocalStack:IsEnabled"]))
                {
                    var config = new AmazonSQSConfig
                    {
                        AuthenticationRegion = configuration["AWS_REGION"],
                        ServiceURL = configuration["LocalStack:ServiceURL"]
                    };
                    return new AmazonSQSClient(config);
                }
                return configuration.GetAWSOptions().CreateServiceClient<IAmazonSQS>();
            })
            .Named<IAmazonSQS>(nameof(IAmazonSQS))
            .SingleInstance();

            // SQS sender → EmailNotificationQueue (Params01)
            builder.RegisterType<AwsSqsMessageSender>()
                .As<IMessageSender>()
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(IAmazonSQS),
                        (_, c) => c.ResolveNamed<IAmazonSQS>(nameof(IAmazonSQS)))
                )
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(AwsSqsMessageSenderParams),
                        (_, c) => c.Resolve<IOptionsSnapshot<AwsSqsMessageSenderParams>>()
                            .Get("AwsSqsMessageSenderParams01"))
                );

            // SNS client (LocalStack-aware)
            builder.Register(componentContext =>
            {
                var configuration = componentContext.Resolve<IConfiguration>();
                if (Convert.ToBoolean(configuration["LocalStack:IsEnabled"]))
                {
                    var config = new AmazonSimpleNotificationServiceConfig
                    {
                        AuthenticationRegion = configuration["AWS_REGION"],
                        ServiceURL = configuration["LocalStack:ServiceURL"]
                    };
                    return new AmazonSimpleNotificationServiceClient(config);
                }
                return configuration.GetAWSOptions().CreateServiceClient<IAmazonSimpleNotificationService>();
            })
            .Named<IAmazonSimpleNotificationService>(nameof(IAmazonSimpleNotificationService))
            .SingleInstance();

            // Lambda client (Instance Profile no AWS, LocalStack-aware localmente)
            builder.Register(componentContext =>
            {
                var configuration = componentContext.Resolve<IConfiguration>();
                if (Convert.ToBoolean(configuration["LocalStack:IsEnabled"]))
                {
                    return new AmazonLambdaClient(new AmazonLambdaConfig
                    {
                        AuthenticationRegion = configuration["AWS_REGION"],
                        ServiceURL           = configuration["LocalStack:ServiceURL"]
                    });
                }
                return new AmazonLambdaClient(); // usa Instance Profile do pod
            })
            .As<IAmazonLambda>()
            .SingleInstance();

            // SNS sender → OrderConfirmedTopic
            builder.RegisterType<AwsSnsMessageSender>()
                .Named<DevOpsProjectEcommerce.SnsHandler.Abstractions.IMessageSender>("SnsOrderConfirmedSender")
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(IAmazonSimpleNotificationService),
                        (_, c) => c.ResolveNamed<IAmazonSimpleNotificationService>(nameof(IAmazonSimpleNotificationService)))
                )
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(AwsSnsMessageParams),
                        (_, c) =>
                        {
                            var topicArn = c.Resolve<IConfiguration>()["Order:AwsSnsParams01:TopicArn"]!;
                            return new AwsSnsMessageParams(topicArn);
                        })
                );
        }
    }
}
