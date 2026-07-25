using Autofac;
using DevOpsProjectEcommerce.IdentityServer.Domain.Services.Contracts;
using DevOpsProjectEcommerce.IdentityServer.Domain.Services.Implementations;

namespace DevOpsProjectEcommerce.IdentityServer.Modules
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UserService>()
                .As<IUserService>()
                .InstancePerLifetimeScope();
        }
    }
}
