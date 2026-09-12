using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// ActivityInstanceExecutionRecord 仓储：6 个影子属性序列化 round-trip，
/// 含空集合写 null 语义与 Update 路径修复。
/// </summary>
public class ActivityInstanceExecutionRecordRepositoryTest : RepositoryTestBase
{
    private static ActivityInstanceExecutionRecord MakeRecord(string marker) => new()
    {
        WorkflowInstanceId = 1,
        ActivityId = $"activity-{marker}",
        ActivityNodeId = $"node-{marker}",
        ActivityType = "TestActivity",
        Status = ActivityStatus.Running,
    };

    [Test]
    public async Task InsertAsync_WithEmptyCollections_WritesNullToShadowProperties()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>();
            var entity = MakeRecord("empty");

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using var verifyScope = CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var tracked = await dbContext.Set<ActivityInstanceExecutionRecord>().FirstOrDefaultAsync(e => e.Id == id);

        // 空集合写 null 语义：所有影子属性均为 null
        foreach (var shadowProperty in new[] { "SerializedActivityState", "SerializedPayload", "SerializedOutputs", "SerializedProperties", "SerializedMetadata", "SerializedException" })
        {
            Assert.That(dbContext.Entry(tracked!).Property(shadowProperty).CurrentValue, Is.Null, shadowProperty);
        }
    }

    [Test]
    public async Task InsertAsync_WithFullState_WritesJsonToAllShadowProperties()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>();
            var entity = MakeRecord("full");
            entity.ActivityState = new Dictionary<string, object?> { ["phase"] = "started" };
            entity.Payload = new Dictionary<string, object> { ["request"] = "r" };
            entity.Outputs = new Dictionary<string, object?> { ["response"] = "ok" };
            entity.Properties = new Dictionary<string, object> { ["retry"] = "no" };
            entity.Metadata = new Dictionary<string, object> { ["actor"] = "tester" };
            entity.Exception = new ExceptionState("System.Exception", "boom", null, null);

            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using var verifyScope = CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var tracked = await dbContext.Set<ActivityInstanceExecutionRecord>().FirstOrDefaultAsync(e => e.Id == id);

        Assert.That(GetShadow(tracked!, "SerializedActivityState", dbContext), Does.Contain("phase"));
        Assert.That(GetShadow(tracked!, "SerializedPayload", dbContext), Does.Contain("request"));
        Assert.That(GetShadow(tracked!, "SerializedOutputs", dbContext), Does.Contain("response"));
        Assert.That(GetShadow(tracked!, "SerializedProperties", dbContext), Does.Contain("retry"));
        Assert.That(GetShadow(tracked!, "SerializedMetadata", dbContext), Does.Contain("actor"));
        Assert.That(GetShadow(tracked!, "SerializedException", dbContext), Does.Contain("boom"));
    }

    [Test]
    public async Task UpdateAsync_WithChangedOutputs_WritesNewJsonToShadowProperty()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>();
            var entity = MakeRecord("update");
            entity.Outputs = new Dictionary<string, object?> { ["step"] = "before" };
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>();
            var entity = await repository.FindAsync(id);

            entity!.Outputs = new Dictionary<string, object?> { ["step"] = "after" };

            await repository.UpdateAsync(entity);
        }

        using var verifyScope = CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
        var tracked = await dbContext.Set<ActivityInstanceExecutionRecord>().FirstOrDefaultAsync(e => e.Id == id);

        // 迁移修复项：Update 路径同样执行序列化
        Assert.That(GetShadow(tracked!, "SerializedOutputs", dbContext), Does.Contain("after"));
    }

    private static string? GetShadow(ActivityInstanceExecutionRecord entity, string propertyName, CikeWorkflowDbContenxt dbContext)
        => (string?)dbContext.Entry(entity).Property(propertyName).CurrentValue;

    [Test]
    public async Task FindAsync_WithCorruptedJson_LogsAndReturnsDefault()
    {
        // 绕过仓储直接写损坏 JSON（模拟历史脏数据）
        await Host.SeedAsync<ActivityInstanceExecutionRecord>(e =>
        {
            e.WorkflowInstanceId = 1;
            e.ActivityId = "activity-corrupt";
            e.ActivityNodeId = "node";
            e.ActivityType = "TestActivity";
        });
        long id;
        using (var scope = CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContenxt>();
            var tracked = await dbContext.Set<ActivityInstanceExecutionRecord>().FirstAsync(e => e.ActivityId == "activity-corrupt");
            dbContext.Entry(tracked).Property("SerializedActivityState").CurrentValue = "{corrupted-json";
            await dbContext.SaveChangesAsync();
            id = tracked.Id;
        }

        // 单查不抛异常，字段保持未还原
        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IActivityInstanceExecutionRecordRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.ActivityState, Is.Null);
    }

    [Test]
    public async Task FindAsync_WithPersistedState_RestoresAllFields()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>();
            var entity = MakeRecord("restore");
            entity.ActivityState = new Dictionary<string, object?> { ["phase"] = "running" };
            entity.Metadata = new Dictionary<string, object> { ["actor"] = "tester" };
            entity.Exception = new ExceptionState("System.TimeoutException", "timeout", "stack", null);
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        var restored = await WithScopeAsync(sp => sp.GetRequiredService<IActivityInstanceExecutionRecordRepository>().FindAsync(id));

        Assert.That(restored, Is.Not.Null);
        Assert.That(restored!.ActivityState, Is.Not.Null);
        Assert.That(restored.Metadata, Is.Not.Null);
        Assert.That(restored.Exception, Is.Not.Null);
        Assert.That(restored.Exception!.TypeName, Is.EqualTo("System.TimeoutException"));
    }
}
