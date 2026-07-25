using Autofac;
using Autofac.Core;
using Microsoft.EntityFrameworkCore;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contexts;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Contracts;
using DevOpsProjectEcommerce.Order.Domain.Repositories.Implementations;
using DevOpsProjectEcommerce.Repositories.Contracts;
using DevOpsProjectEcommerce.Repositories.Implementations;
using DevOpsProjectEcommerce.Shared.HttpHandlers.Contracts;
using DevOpsProjectEcommerce.Shared.InOut.Requests;
using Refit;

namespace DevOpsProjectEcommerce.Order.Modules
{
    public class InfrastructureModule: Module
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
                var options = new DbContextOptionsBuilder<OrderContext>();
                options.UseNpgsql(_configuration.GetConnectionString("Default"));
                var dbContext = new OrderContext(options.Options);
                dbContext.Database.Migrate();
                return dbContext;
            })
            .InstancePerLifetimeScope();

            builder
              .RegisterGeneric(typeof(CreateEntityRepository<>))
              .As(typeof(ICreateEntityRepository<>))
              .WithParameter(
                  new ResolvedParameter(
                      (i, _) => i.ParameterType == typeof(DbContext),
                      (_, c) => c.Resolve<OrderContext>())
              ).InstancePerLifetimeScope();
            
            
            builder
                .RegisterGeneric(typeof(UpdateEntityRepository<>))
                .As(typeof(IUpdateEntityRepository<>))
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(DbContext),
                        (_, c) => c.Resolve<OrderContext>())
                ).InstancePerLifetimeScope();
            
            builder
                .RegisterGeneric(typeof(DeleteEntityRepository<>))
                .As(typeof(IDeleteEntityRepository<>))
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(DbContext),
                        (_, c) => c.Resolve<OrderContext>())
                ).InstancePerLifetimeScope();

            builder
                .RegisterGeneric(typeof(ReadEntityRepository<>))
                .As(typeof(IReadEntityRepository<>))
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(DbContext),
                        (_, c) => c.Resolve<OrderContext>())
                ).InstancePerLifetimeScope();
            
            builder
                .RegisterType<OrderReadRepository>()
                .As<IOrderReadRepository>();

            var hostUrl = _configuration.GetValue<string>("BaseAddress:Identity")!;
            var identityServerApi = RestService.For<IIdentityServerApi>(hostUrl);
            builder.Register(_ => identityServerApi);

            builder.Register( _ =>
            {
                var userRequest = new AuthRequest
                (
                    _configuration.GetValue<string>("Identity:Admin:User")!,
                    _configuration.GetValue<string>("Identity:Admin:User:Password")!,
                    true
                );

                return RestService.For<IMainApi>(
                    _configuration.GetValue<string>("BaseAddress:Main")!,
                    new RefitSettings
                    {
                        AuthorizationHeaderValueGetter = (_,_)=>
                            identityServerApi.AuthAsync(userRequest)
                    });

            }).InstancePerDependency();
        }
    }
}
