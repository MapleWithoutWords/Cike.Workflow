using Cike.AspNetCore.MinimalAPIs.JsonConverts;
using Cike.Core.Modularity;
using Cike.Workflow.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Realtime;

/// <summary>
/// 工作流实时推送模块：SignalR Hub 与执行进度推送中间件都收在此模块内，
/// Core / Application 层不感知 SignalR。Service.Open 引用本模块即完成组装。
/// 对 Application 仅取取消命令类型（项目引用），不依赖其模块加载。
/// </summary>
[DependsOn([
    typeof(CikeWorkflowCoreModule),
    typeof(CikeEventBusLocalModule),
    ])]
public class CikeWorkflowRealtimeModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        // 与 HTTP 层约定一致：long 序列化为字符串，防前端精度丢失
        context.Services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters.Add(new LongToStringConverter());
                options.PayloadSerializerOptions.Converters.Add(new NullableLongToStringConverter());
            });

        // 与 Core 模块的中间件注册方式一致：注册序即管线序，Core 的异常中间件在外、推送在内
        context.Services.Add(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunWorkflowInstanceCommand>), typeof(WorkflowInstancePushMiddleware), ServiceLifetime.Transient));
        context.Services.Add(new ServiceDescriptor(typeof(ILocalEventMiddleware<RunActivityInstanceCommand>), typeof(ActivityInstancePushMiddleware), ServiceLifetime.Transient));
        context.Services.Add(new ServiceDescriptor(typeof(ILocalEventMiddleware<CancelWorkflowCommand>), typeof(CancelWorkflowPushMiddleware), ServiceLifetime.Transient));

        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        // Hub 映射只在 Web 宿主可用；测试等非 Web 宿主拿不到端点路由构建器，跳过即可，
        // 推送走 IHubContext，不依赖 Hub 映射
        var endpointRouteBuilder = context.GetEndpointRouteBuilder();
        endpointRouteBuilder?.MapHub<WorkflowRealtimeHub>(WorkflowRealtimeHub.Path);

        await base.InitializeAsync(context);
    }
}
