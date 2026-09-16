using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.EntityFrameworkCore;

/// <summary>
/// 设计时工厂：供 `dotnet ef migrations` 生成/应用迁移。
/// 仅用于构建模型（迁移生成不连库），连接串与生产方言在工厂内静态指定。
/// </summary>
public class MigrationsDesignTimeFactory : IDesignTimeDbContextFactory<CikeWorkflowDbContext>
{
    public CikeWorkflowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CikeWorkflowDbContext>();
        optionsBuilder.UseMySql(
            "Server=47.120.12.251;Port=6033;Database=cike_workflow;Uid=cike;Pwd=Cike-1471;SslMode=None;Pooling=true;Max Pool Size=200;Allow User Variables=true;",
            new MySqlServerVersion(new Version(8, 0, 36)));

        return new CikeWorkflowDbContext(optionsBuilder.Options, new ServiceCollection().BuildServiceProvider());
    }
}
