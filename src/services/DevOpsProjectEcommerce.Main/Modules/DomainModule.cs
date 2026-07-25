using Autofac;
using Autofac.Core;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.Extensions.Options;
using DevOpsProjectEcommerce.Main.Domain.Commands;
using DevOpsProjectEcommerce.Main.Domain.Tasks;
using DevOpsProjectEcommerce.SqsHandler.Abstractions;
using DevOpsProjectEcommerce.SqsHandler.Implementations;
using DevOpsProjectEcommerce.SqsHandler.Models;

namespace DevOpsProjectEcommerce.Main.Modules
{
    public class DomainModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProductStockProcessor>()
                .Named<IMessageProcessor>(nameof(ProductStockProcessor));
        
            builder.RegisterType<AwsSqsQueueMonitor>()
                .As<IHostedService>()
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(AwsSqsQueueMonitorParams),
                        (_, c) => c.Resolve<IOptionsSnapshot<AwsSqsQueueMonitorParams>>()
                            .Get("AwsSqsQueueMonitorParams01")
                    )
                );
            
            var configuration = MediatRConfigurationBuilder
                  .Create(typeof(RegisterProductCommand).Assembly)
                  .WithAllOpenGenericHandlerTypesRegistered()
                  .Build();

            builder.RegisterMediatR(configuration);
        }
    }
}
