using System.Security.Cryptography.X509Certificates;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DevOpsProjectEcommerce.Main.Middlewares;
using DevOpsProjectEcommerce.Main.Modules;
using DevOpsProjectEcommerce.S3Handler.Models;
using DevOpsProjectEcommerce.SqsHandler.Models;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(LogEventLevel.Information)
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();
try
{
    Log.Information("Starting Main Microservice");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(applicationBuilder =>
    {
        applicationBuilder.RegisterModule(new AwsModule());
        applicationBuilder.RegisterModule<DomainModule>();
        applicationBuilder.RegisterModule(new InfrastructureModule(builder.Configuration));
        applicationBuilder
            .RegisterType<GlobalErrorHandlerMiddleware>()
            .SingleInstance();
    });

    builder.Services.Configure<AwsSqsQueueMonitorParams>(
        "AwsSqsQueueMonitorParams01",
        builder.Configuration.GetSection("Main:AwsSqsQueueMonitorParams01")
    );
    builder.Services.Configure<AwsS3BucketParams>(
        "AwsS3Params01",
        builder.Configuration.GetSection("AwsS3Params01")
    );
    builder.Services.AddMemoryCache();
    builder.Services.AddHealthChecks();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Provide a token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        var passwordBytes = Encoding.UTF8.GetBytes(builder.Configuration["Identity:Key"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration.GetValue<string>("Identity:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Identity:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(passwordBytes),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
    builder.Services.AddAuthorization();
    builder.Host.UseSerilog();

    string pathToCAFile = builder.Configuration["MONGO_CERTIFICATE_CONTAINER_PATH"];
    var localTrustStore = new X509Store(StoreName.Root);
    var certificateCollection = new X509Certificate2Collection();
    try
    {
        certificateCollection.Import(pathToCAFile);
        localTrustStore.Open(OpenFlags.ReadWrite);
        localTrustStore.AddRange(certificateCollection);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Mongo certificate import failed: " + ex.Message);
    }
    finally
    {
        localTrustStore.Close();
    }

    var app = builder.Build();
    app.Map("/main", applicationBuilder =>
    {
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI();
        applicationBuilder.UseRouting();
        applicationBuilder.UseMiddleware<GlobalErrorHandlerMiddleware>();
        applicationBuilder.UseAuthentication();
        applicationBuilder.UseAuthorization();

        applicationBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health",
                new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
        });
    });

    app.Run();
}
catch (HostAbortedException exception)
{
    Log.Warning(exception, "Executing migrations? All good.");
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
// trigger: primeiro run da pipeline
// retry: novo sha para evitar colisao de tag imutavel no ECR
// retry 2: novo sha apos falha transitoria no download do cosign
// retry 3: policy IAM corrigida (ecr:BatchGetImage para cosign sign)
// retry 4: nova tentativa apos falha HTTP transitoria no download do cosign
// retry 5: nova tentativa - instabilidade recorrente do cosign-installer
// retry 6: policy IAM corrigida (ec2:DescribeInstances + elbv2:DescribeLoadBalancers para o tunel SSM)
// retry 7: staging saudavel - pull secret criado + senha do Aurora staging corrigida
// retry 9: continuando apos instabilidade severa e persistente do cosign hoje
