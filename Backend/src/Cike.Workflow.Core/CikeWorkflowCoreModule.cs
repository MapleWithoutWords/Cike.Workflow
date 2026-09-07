using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Workflow.Core.ActivityDescriptors;
using Cike.Workflow.Core.Runners.Internals.Middlewares;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cike.Workflow.Core;

[DependsOn([
    typeof(CikeWorkflowExpressionModule),
    typeof(CikeAuthModule),
    typeof(CikeUniversalIdModule),
    typeof(CikeEventBusLocalModule),
    typeof(CikeCqrsModule),
])]
public class CikeWorkflowCoreModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IStorageDriverRegistry>(StorageDriverRegistry.CreateDefault());
        var storageDriverRegistry = context.Services.GetSingletonInstance<IStorageDriverRegistry>();
        storageDriverRegistry.Add(new StorageDriverDescriptor
        {
            Type = nameof(WorkflowInstanceStorageDriver),
            DisplayName = "Workflow Instance",
            Factory = serviceProvider => serviceProvider.GetRequiredService<WorkflowInstanceStorageDriver>()
        });

        context.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunWorkflowInstanceCommand>), typeof(ExceptionRunWorkflowInstanceMiddleware), ServiceLifetime.Transient));

        context.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunActivityInstanceCommand>), typeof(ExceptionRunActivityInstanceMiddleware), ServiceLifetime.Transient));
        context.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunActivityInstanceCommand>), typeof(ActivityInstanceExecutionLogMiddleware), ServiceLifetime.Transient));
        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var activityProviders = context.ServiceProvider.GetServices<IActivityProvider>();
        var activityRegistry = context.ServiceProvider.GetService<IActivityRegistry>();
        foreach (var provider in activityProviders)
            await activityRegistry.EnsureDescriptorsAsync(provider);

        await base.InitializeAsync(context);
    }
}
