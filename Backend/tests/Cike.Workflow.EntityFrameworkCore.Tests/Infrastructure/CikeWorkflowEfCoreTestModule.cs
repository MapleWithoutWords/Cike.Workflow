using Cike.Caching;
using Cike.Workflow.EntityFrameworkCore;

namespace Cike.Workflow.EntityFrameworkCore.Tests.Infrastructure;

/// <summary>
/// 测试启动模块：加载与生产一致的模块依赖图（数据层全家桶 + 缓存 + 工作流域），
/// 并以 FakeCurrentUser 接管 ICurrentUser、以内存替身接管 ICacheService（避免依赖 HttpContext / Redis）。
/// </summary>
[DependsOn([typeof(CikeWorkflowEntityFrameworkCoreModule)])]
public class CikeWorkflowEfCoreTestModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        // 同实例双注册：ICurrentUser 接口与具体类型解析到同一个 FakeCurrentUser，测试基座无需强转
        context.Services.AddSingleton<FakeCurrentUser>();
        context.Services.Replace(ServiceDescriptor.Singleton<ICurrentUser>(sp => sp.GetRequiredService<FakeCurrentUser>()));

        // 缓存替身（Singleton，模拟 Redis 跨 Scope 可见性）
        context.Services.Replace(ServiceDescriptor.Singleton(typeof(ICacheService<>), typeof(InMemoryCacheService<>)));
        // 定义运行时缓存的真实实现跑在此内存介质上（不依赖 Redis），键/索引/选取逻辑被真实执行
        context.Services.Replace(ServiceDescriptor.Singleton<IMultilevelCacheClient, InMemoryMultilevelCacheClient>());

        // 测试基座不启用环境事务：生产由请求管道的事务中间件统一提交，测试没有该中间件，
        // 不关闭的话仓储 autoSave 写入停留在未提交事务里，Scope 销毁即回滚、跨 Scope 不可见
        context.Services.Configure<UnitOfWorkOptions>(options => options.Enable = false);

        return base.ConfigureServicesAsync(context);
    }
}
