using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Options;
using DevOpsProjectEcommerce.Notificator.Tasks;
using DevOpsProjectEcommerce.SqsHandler.Abstractions;
using DevOpsProjectEcommerce.SqsHandler.Implementations;
using DevOpsProjectEcommerce.SqsHandler.Models;

namespace DevOpsProjectEcommerce.Notificator.Modules;

public class DomainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AwsSesEmailSenderProcessor>()
            .Named<IMessageProcessor>(nameof(AwsSesEmailSenderProcessor));

        builder.RegisterType<AwsSqsQueueMonitor>()
            .As<IHostedService>()
            .WithParameter(
                new ResolvedParameter(
                    (i, _) => i.ParameterType == typeof(AwsSqsQueueMonitorParams),
                    (_, c) => c.Resolve<IOptionsSnapshot<AwsSqsQueueMonitorParams>>()
                        .Get("AwsSqsQueueMonitorParams01")
                )
            );
    }
}
