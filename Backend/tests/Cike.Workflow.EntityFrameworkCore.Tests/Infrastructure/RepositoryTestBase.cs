namespace Cike.Workflow.EntityFrameworkCore.Tests.Infrastructure;

/// <summary>
/// 仓储测试基类：每个测试类独享一个测试宿主（独立 SQLite in-memory 库），类内测试共享、用唯一数据隔离。
/// 数据库类测试统一标注 Integration 分类。
/// </summary>
[Category("Integration")]
public abstract class RepositoryTestBase
{
    private CikeWorkflowEfCoreTestHost? _host;

    protected CikeWorkflowEfCoreTestHost Host => _host ?? throw new InvalidOperationException("Test host is not initialized.");

    [OneTimeSetUp]
    public async Task SetUpHostAsync()
    {
        _host = new CikeWorkflowEfCoreTestHost();
        await _host.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownHostAsync()
    {
        if (_host != null)
        {
            await _host.DisposeAsync();
            _host = null;
        }
    }

    protected IServiceScope CreateScope() => Host.CreateScope();

    /// <summary>在新 Scope 中解析仓储并执行（每个块拿到独立的 DbContext，避免跨用例的跟踪状态泄漏）。</summary>
    protected async Task<T> WithScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = CreateScope();
        return await action(scope.ServiceProvider);
    }
}
