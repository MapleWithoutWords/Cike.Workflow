using Cike.AspNetCore.MinimalAPIs;
using Cike.AspNetCore.MinimalAPIs.Options;
using Cike.AspNetCore.Swagger;
using Cike.Contracts;
using Cike.Core.Modularity;
using Cike.Cqrs;
using Cike.EventBus.Local;
using Elsa.Caching.Options;
using Elsa.Common.Codecs;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Elsa.JavaScript.Libraries.Extensions;
using Elsa.OpenTelemetry.Middleware;
using Elsa.Workflows.CommitStates.Strategies;
using Elsa.Workflows.Features;
using Elsa.Workflows.IncidentStrategies;
using Elsa.Workflows.Options;
using Elsa.Workflows.Runtime.Options;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using Proto.Persistence.MySql;
using System.Text;
using System.Text.Json;

namespace Cike.Workflow.Service.Open;

[DependsOn([
    typeof(CikeContractsModule),
    typeof(CikeCqrsModule),
    typeof(CikeEventBusLocalModule),
    typeof(CikeAspNetCoreMinimalApiModule)
    ])]
public class CikeWorkflowServiceOpenModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.Configure<GlobalMinimalApiRouteOptions>(options =>
        {
            options.EnabledAuthorization = false;
            options.RouteHandlerBuilder = null;
        });

        context.Services.AddHealthChecks();

        context.Services.Configure<MinimalApiOptions>(options =>
        {
            options.LoadMinimalApi(typeof(CikeWorkflowServiceOpenModule).Assembly);
        });
        //context.Services.AddAuthentication();
        context.Services.AddCikeSwagger("Cike.Workflow", options =>
        {
            options.SupportNonNullableReferenceTypes();
        });
        context.Services.AddValidatorsFromAssembly(typeof(CikeWorkflowServiceOpenModule).Assembly);

        context.Services.AddElsa(elsa =>
        {
            elsa.UseJavaScript(config =>
            {

                config.ConfigureJintOptions(jintOptions => jintOptions.AllowClrAccess = true)
                .UseLodashFp()
                    .UseMoment();
            })
            .UseCSharp()
            .UseLiquid()
            .UseHttp(config =>
            {
                config.UseCache();
            })
            //.UseFlowchart(flowchart => flowchart.UseTokenBasedExecution())
            .UseWorkflows(workflowFeature =>
            {
                workflowFeature.WithDefaultWorkflowExecutionPipeline(pipeline => pipeline.UseWorkflowExecutionTracing());
                workflowFeature.WithDefaultActivityExecutionPipeline(pipeline => pipeline.UseActivityExecutionTracing());
                workflowFeature.UseCommitStrategies(strategies =>
                    {
                        strategies.AddStandardStrategies();
                        strategies.Add("Every 10 seconds", new PeriodicWorkflowStrategy(TimeSpan.FromSeconds(10)));
                    });
                })
            .UseWorkflowManagement(manage =>
                {
                    manage.UseEntityFrameworkCore(persistenceFeature =>
                    {
                        persistenceFeature.UseMySql(configuration["ConnectionStrings:CikeWorkflow"]);
                        persistenceFeature.RunMigrations = true;
                    });

                    manage.SetCompressionAlgorithm(nameof(Zstd));
                    manage.UseCache();
                })
            .UseWorkflowRuntime(runtime =>
                {
                    runtime.UseProtoActor(configure =>
                    {
                    });
                    runtime.UseEntityFrameworkCore(persistenceFeature =>
                    {
                        persistenceFeature.UseMySql(configuration["ConnectionStrings:CikeWorkflow"]);
                        persistenceFeature.RunMigrations = true;
                    });

                })
            .UseOpenTelemetry(otel => otel.UseNewRootActivityForRemoteParent = true)
            .UseDistributedCache(distributedCaching =>
            {
                distributedCaching.UseProtoActor();
            })
            .UseProtoActor(proto =>
            {
                proto
                    .EnableMetrics()
                    .EnableTracing();
                proto.PersistenceProvider = _ => new MySqlProvider(configuration["ConnectionStrings:CikeWorkflow"], "cike", "cike_actor_event", obj=> JsonSerializer.Serialize(obj), str => JsonSerializer.Deserialize<object>(str));
            });

        });


        context.Services.Configure<RuntimeOptions>(options => { options.InactivityThreshold = TimeSpan.FromMinutes(15); });
        context.Services.Configure<BookmarkQueuePurgeOptions>(options => options.Ttl = TimeSpan.FromMinutes(10));
        context.Services.Configure<CachingOptions>(options => options.CacheDuration = TimeSpan.FromDays(1));
        context.Services.Configure<IncidentOptions>(options => options.DefaultIncidentStrategy = typeof(ContinueWithIncidentsStrategy));

        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var endpointRouteBuilder = context.GetEndpointRouteBuilder();
        var appBuilder = context.GetApplicationBuilder();
#if DEBUG
        appBuilder.UseCikeSwaggerUI("Cike.Workflow");
#endif
        endpointRouteBuilder.MapHealthChecks("/health");
        appBuilder.UseWorkflows();
        appBuilder.UseJsonSerializationErrorHandler();

        await base.InitializeAsync(context);
    }
}
