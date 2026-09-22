using Cike.Application;
using Cike.Caching;
using Cike.Core.Modularity;
using Cike.Uow;
using Cike.Workflow.Application;
using Cike.Workflow.EntityFrameworkCore;
using Cike.Workflow.Realtime.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

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

/// <summary>
/// 全宿主测试模块（SQLite in-memory）：Application 全链路 + Realtime，
/// 用于经公共命令流验证取消推送等依赖数据层的行为。
/// </summary>
[DependsOn([
    typeof(CikeWorkflowRealtimeModule),
    typeof(CikeWorkflowApplicationModule),
    typeof(CikeWorkflowEntityFrameworkCoreModule),
    ])]
internal class CikeWorkflowRealtimeApplicationTestModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddLogging();

        // 同实例双注册：ICurrentUser 接口与具体类型解析到同一个 FakeCurrentUser；
        // Id 须为合法 Guid（定义发布流程会取当前用户 Guid 落审计字段）
        context.Services.AddSingleton(new FakeCurrentUser { Id = Guid.NewGuid().ToString() });
        context.Services.Replace(ServiceDescriptor.Singleton<ICurrentUser>(sp => sp.GetRequiredService<FakeCurrentUser>()));

        // 缓存替身（Singleton，模拟 Redis 跨 Scope 可见性）
        context.Services.Replace(ServiceDescriptor.Singleton(typeof(ICacheService<>), typeof(InMemoryCacheService<>)));
        context.Services.Replace(ServiceDescriptor.Singleton<IMultilevelCacheClient, InMemoryMultilevelCacheClient>());

        // 测试基座不启用环境事务：生产由请求管道的事务中间件统一提交
        context.Services.Configure<UnitOfWorkOptions>(options => options.Enable = false);

        CikeWorkflowRealtimeCoreTestModule.ReplaceHubContext(context.Services);

        return base.ConfigureServicesAsync(context);
    }
}
