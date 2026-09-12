namespace Cike.Workflow.EntityFrameworkCore.Tests.Infrastructure;

/// <summary>
/// 仓储测试基座：以与生产一致的方式（模块加载 + AddCikeDbContext）组装容器。
/// 数据库为共享单连接的 SQLite in-memory——连接存活期间库不丢失，跨 Scope 可见已提交数据。
/// 每个测试类一个实例（RepositoryTestBase 创建），类内测试共享同一数据库，用唯一数据隔离。
/// 影子属性的 json 列类型在 SQLite 下按 TEXT 类型亲和性建表，生产 MySQL 映射不受影响。
/// </summary>
public sealed class CikeWorkflowEfCoreTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private ServiceProvider _serviceProvider = default!;

    public IServiceProvider ServiceProvider => _serviceProvider;

    public async Task InitializeAsync()
    {
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();

        // 空配置占位：模块加载阶段解析连接串（SQLite 覆盖后连接串本身不被使用）
        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:CikeWorkflowDbContenxt"] = "DataSource=:memory:";
        configuration["ConnectionStrings:Project"] = "DataSource=:memory:";
        services.ReplaceConfiguration(configuration);

        // 与生产一致：模块加载（依赖图 + 约定注册）走 AddApplicationAsync 扩展
        await services.AddApplicationAsync<CikeWorkflowEfCoreTestModule>();

        // 测试专用：把共享的 in-memory 连接注入 DbContextOptions，覆盖模块默认的 MySQL 方言
        services.Configure<CikeDbContextOptions>(options =>
        {
            options.Configure(context => context.DbContextOptionsBuilder.UseSqlite(_connection));
        });

        _serviceProvider = services.BuildServiceProvider();

        // 建表
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        _connection.Dispose();
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    /// <summary>
    /// 绕过仓储直接写库（构造影子属性损坏 JSON 等特殊数据用）。
    /// 显式设置的非默认审计字段（如 CreatedAt）会被保留，可用于排序测试数据构造。
    /// </summary>
    public async Task SeedAsync<TEntity>(Action<TEntity>? configure, CancellationToken cancellationToken = default) where TEntity : class, new()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var entity = new TEntity();
        configure?.Invoke(entity);
        dbContext.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
