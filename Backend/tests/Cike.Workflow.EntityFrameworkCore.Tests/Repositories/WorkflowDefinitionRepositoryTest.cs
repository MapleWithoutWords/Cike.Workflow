using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// WorkflowDefinition 仓储：影子属性（SerializedOptions）序列化 round-trip——
/// 插入写 JSON、更新写新值 JSON（迁移修复项）、单查还原、损坏 JSON 容错、排序兼容重载。
/// </summary>
public class WorkflowDefinitionRepositoryTest : RepositoryTestBase
{
    private static WorkflowDefinition MakeDefinition(string marker) => new()
    {
        WorkspaceId = 1,
        DefinitionId = $"WF_{marker}",
        Name = $"def-{marker}",
        Description = "test",
    };

    /// <summary>在新 Scope 中读取影子属性原始值（跟踪查询，影子属性值随实体条目携带）。</summary>
    private async Task<string?> GetRawSerializedOptionsAsync(long id)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var entity = await dbContext.Set<WorkflowDefinition>().FirstOrDefaultAsync(e => e.Id == id);
        return entity == null ? null : (string?)dbContext.Entry(entity).Property("SerializedOptions").CurrentValue;
    }

    [Test]
    public async Task InsertAsync_WithOptions_WritesJsonToShadowProperty()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
            var entity = MakeDefinition("insert");
            entity.Options = new WorkflowDefinitionOptionsValueObject { Outcomes = ["done", "retry"] };

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        var raw = await GetRawSerializedOptionsAsync(id);
        Assert.That(raw, Is.Not.Null);
        Assert.That(raw, Does.Contain("done"));
    }

    [Test]
    public async Task UpdateAsync_WithChangedOptions_WritesNewJsonToShadowProperty()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
            var entity = MakeDefinition("update");
            entity.Options = new WorkflowDefinitionOptionsValueObject { Outcomes = ["v1"] };

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
            var entity = await repository.FindAsync(id);

            entity!.Options = new WorkflowDefinitionOptionsValueObject { Outcomes = ["v1", "v2"] };

            await repository.UpdateAsync(entity);
        }

        // 迁移修复项：Update 路径同样执行序列化，影子属性写入新值 JSON
        var raw = await GetRawSerializedOptionsAsync(id);
        Assert.That(raw, Does.Contain("v2"));
    }

    [Test]
    public async Task FindAsync_WithPersistedOptions_RestoresOptions()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
            var entity = MakeDefinition("restore");
            entity.Options = new WorkflowDefinitionOptionsValueObject { Outcomes = ["restored"] };
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IWorkflowDefinitionRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.Options.Outcomes, Does.Contain("restored"));
    }

    [Test]
    public async Task FindAsync_WithCorruptedJson_LogsAndReturnsDefault()
    {
        // 绕过仓储直接写损坏 JSON（模拟历史脏数据）
        await Host.SeedAsync<WorkflowDefinition>(e =>
        {
            e.WorkspaceId = 1;
            e.DefinitionId = "WF_corrupt";
            e.Name = "def-corrupt";
            e.Description = "test";
        });
        long id;
        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
            var tracked = await dbContext.Set<WorkflowDefinition>().FirstAsync(e => e.DefinitionId == "WF_corrupt");
            dbContext.Entry(tracked).Property("SerializedOptions").CurrentValue = "{corrupted-json";
            await dbContext.SaveChangesAsync();
            id = tracked.Id;
        }

        // 单查不抛异常，Options 回退默认值
        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IWorkflowDefinitionRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.Options, Is.Not.Null);
    }

    [Test]
    public async Task GetListAsync_WithExplicitSorting_SortsByGivenExpression()
    {
        await Host.SeedAsync<WorkflowDefinition>(e => { e.WorkspaceId = 2; e.DefinitionId = "WF_sort_1"; e.Name = "s1"; e.Description = "d"; e.Version = 1; e.CreatedAt = new DateTime(2024, 1, 1); });
        await Host.SeedAsync<WorkflowDefinition>(e => { e.WorkspaceId = 2; e.DefinitionId = "WF_sort_2"; e.Name = "s2"; e.Description = "d"; e.Version = 3; e.CreatedAt = new DateTime(2024, 1, 2); });
        await Host.SeedAsync<WorkflowDefinition>(e => { e.WorkspaceId = 2; e.DefinitionId = "WF_sort_3"; e.Name = "s3"; e.Description = "d"; e.Version = 2; e.CreatedAt = new DateTime(2024, 1, 3); });

        var versions = await WithScopeAsync(sp => sp.GetRequiredService<IWorkflowDefinitionRepository>()
            .GetListAsync(e => e.WorkspaceId == 2, "Version desc"));

        Assert.That(versions.Select(e => e.Version), Is.EqualTo(new[] { 3, 2, 1 }));
    }

    [Test]
    public async Task GetListAsync_WithDefaultSorting_SortsByCreatedAtDesc()
    {
        await Host.SeedAsync<WorkflowDefinition>(e => { e.WorkspaceId = 3; e.DefinitionId = "WF_defsort_1"; e.Name = "d1"; e.Description = "d"; e.CreatedAt = new DateTime(2024, 1, 1); });
        await Host.SeedAsync<WorkflowDefinition>(e => { e.WorkspaceId = 3; e.DefinitionId = "WF_defsort_2"; e.Name = "d2"; e.Description = "d"; e.CreatedAt = new DateTime(2024, 1, 2); });
        await Host.SeedAsync<WorkflowDefinition>(e => { e.WorkspaceId = 3; e.DefinitionId = "WF_defsort_3"; e.Name = "d3"; e.Description = "d"; e.CreatedAt = new DateTime(2024, 1, 3); });

        var items = await WithScopeAsync(sp => sp.GetRequiredService<IWorkflowDefinitionRepository>()
            .GetListAsync(e => e.WorkspaceId == 3));

        Assert.That(items.Select(e => e.Name), Is.EqualTo(new[] { "d3", "d2", "d1" }));
    }
}
