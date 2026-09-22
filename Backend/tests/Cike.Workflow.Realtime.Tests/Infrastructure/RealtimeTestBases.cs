using Cike.Core.Extensions;
using Cike.Core.Modularity;
using Cike.Core.ObjectAccessor;
using Cike.Workflow.Realtime.Tests.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

/// <summary>
/// 全宿主基类：Application 全链路 + Realtime + SQLite in-memory 共享单连接，
/// 与生产一致地组装模块；取消推送等依赖数据层的行为经公共命令流在此验证。
/// 每个测试类独享一个宿主（独立 in-memory 库）。
/// </summary>
public abstract class RealtimeApplicationTestBase
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private ServiceProvider _serviceProvider = default!;

    protected IServiceProvider Services => _serviceProvider;

    protected RealtimeHubProbe Probe { get; }

    protected RealtimeApplicationTestBase()
    {
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();

        // 空配置占位：模块加载阶段解析连接串（SQLite 覆盖后连接串本身不被使用）
        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:CikeWorkflowDbContenxt"] = "DataSource=:memory:";
        configuration["ConnectionStrings:Project"] = "DataSource=:memory:";
        services.ReplaceConfiguration(configuration);

        Probe = new RealtimeHubProbe();
        services.AddSingleton(Probe);

        RealtimeHostDefaults.Apply(services);
        services.AddApplicationAsync<CikeWorkflowRealtimeApplicationTestModule>().GetAwaiter().GetResult();

        // 测试专用：把共享的 in-memory 连接注入 DbContextOptions，覆盖模块默认的 MySQL 方言
        services.Configure<CikeDbContextOptions>(options =>
        {
            options.Configure(context => context.DbContextOptionsBuilder.UseSqlite(_connection));
        });

        _serviceProvider = services.BuildServiceProvider();

        var application = _serviceProvider.GetRequiredService<IApplicationWithExternalServiceProvider>();
        application.InitializeAsync(_serviceProvider).ConfigureAwait(false).GetAwaiter().GetResult();

        // 建表
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContext>();
            dbContext.Database.EnsureCreatedAsync().GetAwaiter().GetResult();
        }
    }
}
