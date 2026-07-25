using Amazon.S3;
using Amazon.SQS;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Options;
using DevOpsProjectEcommerce.S3Handler.Abstractions;
using DevOpsProjectEcommerce.S3Handler.Implementations;
using DevOpsProjectEcommerce.S3Handler.Models;

namespace DevOpsProjectEcommerce.Main.Modules
{
    public class AwsModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(componentContext =>
                {
                    var configuration = componentContext.Resolve<IConfiguration>();
                    if (Convert.ToBoolean(configuration["LocalStack:IsEnabled"])){
                        var config = new AmazonSQSConfig
                        {
                            AuthenticationRegion = configuration["AWS_REGION"],
                            ServiceURL = configuration["LocalStack:ServiceURL"]
                        };

                        return new AmazonSQSClient(config);
                    }

                    var client = configuration
                        .GetAWSOptions()
                        .CreateServiceClient<IAmazonSQS>();

                    return client;
                })
                .Named<IAmazonSQS>(nameof(IAmazonSQS))
                .SingleInstance();

            builder.RegisterType<AmazonS3Client>()
                .As<IAmazonS3>();
                
            builder.RegisterType<AwsS3ObjectManager>()
                .As<IObjectManager>()
                .WithParameter(
                    new ResolvedParameter(
                        (i, _) => i.ParameterType == typeof(AwsS3BucketParams),
                        (_, c) => c.Resolve<IOptionsSnapshot<AwsS3BucketParams>>()
                            .Get("AwsS3Params01"))
                );
        }
    }
}
