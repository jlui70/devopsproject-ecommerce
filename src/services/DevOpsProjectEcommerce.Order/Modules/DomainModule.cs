using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using DevOpsProjectEcommerce.Order.Domain.Commands;

namespace DevOpsProjectEcommerce.Order.Modules
{
    public class DomainModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = MediatRConfigurationBuilder
                       .Create(typeof(CreateOrderCommand).Assembly)
                       .WithAllOpenGenericHandlerTypesRegistered()
                       .Build();

            builder.RegisterMediatR(configuration);
        }
    }
}
