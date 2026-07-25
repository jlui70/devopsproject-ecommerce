using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Options;
using DevOpsProjectEcommerce.InvoiceGenerator.Tasks;
using DevOpsProjectEcommerce.SqsHandler.Abstractions;
using DevOpsProjectEcommerce.SqsHandler.Implementations;
using DevOpsProjectEcommerce.SqsHandler.Models;

namespace DevOpsProjectEcommerce.InvoiceGenerator.Modules;

public class DomainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<InvoiceProcessor>()
            .Named<IMessageProcessor>(nameof(InvoiceProcessor));
        
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
