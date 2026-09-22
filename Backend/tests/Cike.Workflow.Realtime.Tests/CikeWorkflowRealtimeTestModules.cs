using Cike.Core.Modularity;
using Cike.Workflow.Realtime.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Cike.Uow;

namespace Cike.Workflow.Realtime.Tests;

/// <summary>
/// 轻量宿主测试模块（无数据库）：Core + Realtime，IUnitOfWork 用替身，
/// 用于经真实 IWorkflowRunner 验证运行推送序列。
/// </summary>
[DependsOn([
    typeof(CikeWorkflowCoreModule),
    typeof(CikeWorkflowRealtimeModule),
    ])]
internal class CikeWorkflowRealtimeCoreTestModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddLogging();
        context.Services.AddSingleton(Substitute.For<IUnitOfWork>());
        ReplaceHubContext(context.Services);
        await base.ConfigureServicesAsync(context);
    }

    internal static void ReplaceHubContext(IServiceCollection services)
        => services.Replace(ServiceDescriptor.Singleton<IHubContext<WorkflowRealtimeHub>>(
            sp => sp.GetRequiredService<RealtimeHubProbe>().Context));
}
