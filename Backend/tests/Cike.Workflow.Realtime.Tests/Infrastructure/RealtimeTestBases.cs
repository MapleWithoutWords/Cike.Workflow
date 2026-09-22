using Cike.Core.Extensions;
using Cike.Core.Modularity;
using Cike.Core.ObjectAccessor;
using Cike.Workflow.Realtime.Tests.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Realtime.Tests;

/// <summary>
/// 非测试逻辑的宿主组装公共项：空端点路由构建器访问器。
/// 空访问器模拟"非 Web 宿主"：Realtime 模块初始化时取到 null 路由构建器、跳过 Hub 映射。
/// </summary>
internal static class RealtimeHostDefaults
{
    internal static void Apply(IServiceCollection services)
    {
        services.AddSingleton<IObjectAccessor<IEndpointRouteBuilder>>(new ObjectAccessor<IEndpointRouteBuilder>());
    }
}

/// <summary>
/// 轻量宿主基类：Core + Realtime 模块直连 DI，经真实 IWorkflowRunner 触发推送链路，
/// 不落数据库（NoopCommitStateHandler 生效）。每个测试类独享一个宿主。
/// </summary>
public abstract class RealtimeCoreTestBase
{
    protected IServiceProvider Services { get; }

    protected RealtimeHubProbe Probe { get; }

    protected IWorkflowRunner Runner => Services.GetRequiredService<IWorkflowRunner>();

    [SetUp]
    public void ResetProbe()
    {
        Probe.Events.Clear();
        Probe.Groups.Clear();
    }

    protected RealtimeCoreTestBase()
    {
        var services = new ServiceCollection();
        services.ReplaceConfiguration(new ConfigurationManager());
        Probe = new RealtimeHubProbe();
        services.AddSingleton(Probe);
        RealtimeHostDefaults.Apply(services);
        services.AddApplicationAsync<CikeWorkflowRealtimeCoreTestModule>().GetAwaiter().GetResult();

        Services = services.BuildServiceProvider();

        var application = Services.GetRequiredService<IApplicationWithExternalServiceProvider>();
        application.InitializeAsync(Services).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
