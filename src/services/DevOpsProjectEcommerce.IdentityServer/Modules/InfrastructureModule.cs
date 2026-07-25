using Autofac;
using Autofac.Core;
using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.IdentityServer.Domain.Repositories.Contexts;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Implementations;

namespace DevOpsProjectEcommerce.IdentityServer.Modules
{
    public class InfrastructureModule : Module
    {
        private readonly IConfiguration _configuration;

        public InfrastructureModule(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ =>
            {
                var options = new DbContextOptionsBuilder<IdentityServerContext>();
                options.UseNpgsql(_configuration.GetConnectionString("Default"));
                var dbContext = new IdentityServerContext(options.Options, _configuration);
                dbContext.Database.Migrate();
                return dbContext;
            })
            .InstancePerLifetimeScope();

            builder
                .RegisterGeneric(typeof(ReadEntityRepository<>))
                .As(typeof(IReadEntityRepository<>))
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(DbContext),
                        (_, c) => c.Resolve<IdentityServerContext>())
                )
                .InstancePerLifetimeScope();
        }
    }
}
