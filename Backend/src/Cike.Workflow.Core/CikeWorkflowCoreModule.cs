using Cike.Core.Hashers;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Workflow.Core.ActivityDescriptors;
using Cike.Workflow.Core.ActivityDescriptors.Internals;
using Cike.Workflow.Core.Expressions;
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
        context.Services.AddSingleton<IActivityDescriber>(ActivityDescriber.CreateInstance());
        context.Services.AddSingleton<IActivityRegistry>(new ActivityRegistry(context.Services.GetSingletonInstance<IActivityDescriber>(), context.Services.GetSingletonInstance<ICurrentTenantAccessor>()));

        context.Services.AddSingleton<IStorageDriverRegistry>(StorageDriverRegistry.CreateDefault());
        var storageDriverRegistry = context.Services.GetSingletonInstance<IStorageDriverRegistry>();
        storageDriverRegistry.Add(new StorageDriverDescriptor
        {
            Type = nameof(WorkflowInstanceStorageDriver),
            DisplayName = "Workflow Instance",
            Factory = serviceProvider => serviceProvider.GetRequiredService<WorkflowInstanceStorageDriver>()
        });

        context.Services.Add(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunWorkflowInstanceCommand>), typeof(ExceptionRunWorkflowInstanceMiddleware), ServiceLifetime.Transient));

        context.Services.Add(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunActivityInstanceCommand>), typeof(ExceptionRunActivityInstanceMiddleware), ServiceLifetime.Transient));
        context.Services.Add(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunActivityInstanceCommand>), typeof(ActivityInstanceExecutionLogMiddleware), ServiceLifetime.Transient));

        var expressionDescriptorRegistry = context.Services.GetSingletonInstance<IExpressionDescriptorRegistry>();
        expressionDescriptorRegistry.AddRange([new ExpressionDescriptor
        {
            Type = "Variable",
            DisplayName = "变量",
            Icon = "variable",
            HandlerFactory = serviceProvider => ActivatorUtilities.GetServiceOrCreateInstance<VariableExpressionHandler>(serviceProvider)
        },new ExpressionDescriptor
        {
            Type = "Input",
            DisplayName = "输入参数",
            Icon = "log-in",
            HandlerFactory = serviceProvider => ActivatorUtilities.GetServiceOrCreateInstance<WorkflowInputExpressionHandler>(serviceProvider)
        }]);

        // Cike.Core 自身无模块类、约定扫描不覆盖，IHasher 需手动注册（framework-core.md 反模式节；实测 1.0.60023 同样行为）
        context.Services.AddSingleton<IHasher, Hasher>();
        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        // IActivityProvider 可能是 Scoped 注册（如 TypedActivityProvider），根容器直接解析会抛异常
        using var scope = context.ServiceProvider.CreateScope();
        var activityProviders = scope.ServiceProvider.GetServices<IActivityProvider>();
        var activityRegistry = scope.ServiceProvider.GetService<IActivityRegistry>();
        foreach (var provider in activityProviders)
            await activityRegistry!.EnsureDescriptorsAsync(provider);

        await base.InitializeAsync(context);
    }
}
