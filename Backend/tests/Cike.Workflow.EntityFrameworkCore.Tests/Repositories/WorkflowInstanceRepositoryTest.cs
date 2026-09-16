using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// WorkflowInstance 仓储：影子属性（SerializedWorkflowState）序列化 round-trip。
/// </summary>
public class WorkflowInstanceRepositoryTest : RepositoryTestBase
{
    private static WorkflowInstance MakeInstance(string marker, WorkflowState state) => new()
    {
        WorkspaceId = 1,
        DefinitionId = $"WF_{marker}",
        Name = marker,
        WorkflowState = state,
    };

    private async Task<string?> GetRawStateAsync(long id)
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContext>();
        var entity = await dbContext.Set<WorkflowInstance>().FirstOrDefaultAsync(e => e.Id == id);
        return entity == null ? null : (string?)dbContext.Entry(entity).Property("SerializedWorkflowState").CurrentValue;
    }

    [Test]
    public async Task InsertAsync_WithWorkflowState_WritesJsonToShadowProperty()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
            var state = new WorkflowState { DefinitionId = "WF_insert", Status = WorkflowStatus.Executing, Input = new Dictionary<string, object> { ["amount"] = 100 } };
            var entity = MakeInstance("insert", state);

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        var raw = await GetRawStateAsync(id);
        Assert.That(raw, Is.Not.Null);
        Assert.That(raw, Does.Contain("amount"));
    }

    [Test]
    public async Task UpdateAsync_WithChangedState_WritesNewJsonToShadowProperty()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
            var state = new WorkflowState { DefinitionId = "WF_update", Status = WorkflowStatus.Pending };
            var entity = MakeInstance("update", state);
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
            var entity = await repository.FindAsync(id);

            entity!.WorkflowState.Output = new Dictionary<string, object> { ["result"] = "ok" };

            await repository.UpdateAsync(entity);
        }

        // 迁移修复项：Update 路径同样执行序列化，影子属性写入新值 JSON
        var raw = await GetRawStateAsync(id);
        Assert.That(raw, Does.Contain("result"));
    }

    [Test]
    public async Task FindAsync_WithPersistedState_RestoresWorkflowState()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
            var state = new WorkflowState { DefinitionId = "WF_restore", Status = WorkflowStatus.Suspended, Input = new Dictionary<string, object> { ["key"] = "value" } };
            var entity = MakeInstance("restore", state);
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IWorkflowInstanceRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Suspended));
        Assert.That(restored.WorkflowState.Input, Contains.Key("key"));
    }

    [Test]
    public async Task FindAsync_WithCorruptedJson_LogsAndReturnsDefault()
    {
        // 绕过仓储直接写损坏 JSON（模拟历史脏数据）
        await Host.SeedAsync<WorkflowInstance>(e =>
        {
            e.WorkspaceId = 1;
            e.DefinitionId = "WF_corrupt";
            e.Name = "corrupt";
        });
        long id;
        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContext>();
            var tracked = await dbContext.Set<WorkflowInstance>().FirstAsync(e => e.DefinitionId == "WF_corrupt");
            dbContext.Entry(tracked).Property("SerializedWorkflowState").CurrentValue = "{corrupted-json";
            await dbContext.SaveChangesAsync();
            id = tracked.Id;
        }

        // 单查不抛异常，状态回退为未还原
        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IWorkflowInstanceRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.WorkflowState, Is.Null);
    }
}
