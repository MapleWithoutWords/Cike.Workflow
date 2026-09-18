using System.Net;
using Cike.EntityFrameworkCore;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Domain.Data;
using Cike.Workflow.Domain.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Service.Open.Tests.WorkflowInstances;

/// <summary>
/// 工作流实例查询集成测试：分页列表（WorkflowInstanceFilter）与详情（状态快照 + 活动执行记录）。
/// 定义行走 API 构造；实例与活动记录经仓储播种（实例的 WorkflowState 影子属性序列化必须走仓储写路径）。
/// </summary>
[Category("Integration")]
internal class WorkflowInstanceQueryTest : BaseIntegrationTest
{
    private const string DefinitionName = "实例查询_测试流程";

    private async Task<(string DefinitionId, long DefinitionVersionId)> PrepareDefinitionAsync()
    {
        var client = CreateClient();
        var workspaceResponse = await client.PostAsJsonAsync("/api/v1/Workspaces", new
        {
            code = $"WS_{Guid.NewGuid():N}",
            name = $"工作空间_{Guid.NewGuid():N}".Substring(0, 20),
            description = "集成测试",
        });
        await EnsureSuccessAsync(workspaceResponse);
        var workspaceId = long.Parse((await workspaceResponse.Content.ReadAsStringAsync()).Trim('"'));

        var definitionId = $"WF_{Guid.NewGuid():N}";
        var definitionResponse = await client.PostAsJsonAsync("/api/v1/WorkflowDefinitions", new
        {
            workspaceId,
            folderId = 0,
            definitionId,
            name = DefinitionName,
            description = "集成测试",
        });
        await EnsureSuccessAsync(definitionResponse);
        var definitionVersionId = long.Parse((await definitionResponse.Content.ReadAsStringAsync()).Trim('"'));

        return (definitionId, definitionVersionId);
    }

    private async Task<long> SeedInstanceAsync((string DefinitionId, long DefinitionVersionId) definition,
        int version, string name, WorkflowStatus status, string? correlationId = null,
        Action<WorkflowInstance>? configure = null)
    {
        var instance = new WorkflowInstance
        {
            DefinitionId = definition.DefinitionId,
            DefinitionVersionId = definition.DefinitionVersionId,
            Version = version,
            Name = name,
            Status = status,
            CorrelationId = correlationId ?? string.Empty,
            IsExecuting = status is WorkflowStatus.Executing or WorkflowStatus.Pending,
            WorkflowState = new WorkflowState
            {
                DefinitionId = definition.DefinitionId,
                DefinitionVersionId = definition.DefinitionVersionId,
                DefinitionVersion = version,
                Name = name,
                Status = status,
                CorrelationId = correlationId ?? string.Empty,
                Input = new Dictionary<string, object> { ["orderId"] = "A-001" },
            },
        };
        configure?.Invoke(instance);

        using var scope = _rootServices.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceRepository>();
        await repository.InsertAsync(instance);
        await CommitScopeAsync(scope);
        return instance.Id;
    }

    private async Task SeedActivityRecordsAsync(long workflowInstanceId,
        params ActivityInstanceExecutionRecord[] records)
    {
        using var scope = _rootServices.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IActivityInstanceExecutionRecordRepository>();
        foreach (var record in records)
        {
            record.WorkflowInstanceId = workflowInstanceId;
            await repository.InsertAsync(record);
        }
        await CommitScopeAsync(scope);
    }

    private async Task CommitScopeAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CikeWorkflowDbContext>();
        if (dbContext.Database.CurrentTransaction != null)
            await dbContext.Database.CurrentTransaction.CommitAsync();
    }

    private Task<HttpResponseMessage> PostPagedListAsync(object? filter, int page = 1, int pageSize = 10)
        => CreateClient().PostAsJsonAsync($"/api/v1/WorkflowInstances/PagedList?page={page}&pageSize={pageSize}",
            filter ?? new { });

    private async Task<JsonDocument> PostPagedListForDocAsync(object? filter, int page = 1, int pageSize = 10)
    {
        var response = await PostPagedListAsync(filter, page, pageSize);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<JsonDocument>())!;
    }

    [Test]
    public async Task PostPagedListAsync_WithoutFilter_ReturnsPagedInstancesWithDefinitionName()
    {
        var definition = await PrepareDefinitionAsync();
        await SeedInstanceAsync(definition, 1, "实例_甲", WorkflowStatus.Finished);
        await SeedInstanceAsync(definition, 1, "实例_乙", WorkflowStatus.Executing);
        await SeedInstanceAsync(definition, 1, "实例_丙", WorkflowStatus.Faulted);

        var doc = await PostPagedListForDocAsync(new { definitionId = definition.DefinitionId }, page: 1, pageSize: 2);
        var root = doc.RootElement;

        Assert.That(GetLong(root, "total"), Is.EqualTo(3));
        var items = root.GetProperty("items").EnumerateArray().ToList();
        Assert.That(items, Has.Count.EqualTo(2));
        foreach (var item in items)
        {
            Assert.That(GetString(item, "definitionName"), Is.EqualTo(DefinitionName));
            Assert.That(GetString(item, "definitionId"), Does.StartWith("WF_"));
            Assert.That(GetLong(item, "definitionVersionId"), Is.EqualTo(definition.DefinitionVersionId));
            Assert.That(item.GetProperty("status").GetInt32(),
                Is.EqualTo((int)WorkflowStatus.Finished).Or.EqualTo((int)WorkflowStatus.Executing).Or.EqualTo((int)WorkflowStatus.Faulted));
        }
    }

    [Test]
    public async Task PostPagedListAsync_WithStatusFilter_ReturnsOnlyMatchingInstances()
    {
        var definition = await PrepareDefinitionAsync();
        await SeedInstanceAsync(definition, 1, "实例_完成", WorkflowStatus.Finished);
        await SeedInstanceAsync(definition, 1, "实例_故障", WorkflowStatus.Faulted);

        var doc = await PostPagedListForDocAsync(new { definitionId = definition.DefinitionId, workflowStatus = (int)WorkflowStatus.Faulted });

        Assert.That(GetLong(doc.RootElement, "total"), Is.EqualTo(1));
        var item = doc.RootElement.GetProperty("items").EnumerateArray().Single();
        Assert.That(item.GetProperty("status").GetInt32(), Is.EqualTo((int)WorkflowStatus.Faulted));
        Assert.That(GetString(item, "name"), Is.EqualTo("实例_故障"));
    }

    [Test]
    public async Task PostPagedListAsync_WithSearchTerm_ReturnsInstancesMatchingName()
    {
        var definition = await PrepareDefinitionAsync();
        var marker = $"甲_{Guid.NewGuid():N}"[..8];
        await SeedInstanceAsync(definition, 1, $"订单审批_{marker}", WorkflowStatus.Executing);
        await SeedInstanceAsync(definition, 1, "对账批次", WorkflowStatus.Finished);

        var doc = await PostPagedListForDocAsync(new { searchTerm = marker });

        Assert.That(GetLong(doc.RootElement, "total"), Is.EqualTo(1));
        Assert.That(GetString(doc.RootElement.GetProperty("items").EnumerateArray().Single(), "name"),
            Does.Contain(marker));
    }

    [Test]
    public async Task PostPagedListAsync_WithTimestampFilter_FiltersByCreatedAt()
    {
        var definition = await PrepareDefinitionAsync();
        await SeedInstanceAsync(definition, 1, "实例_时间", WorkflowStatus.Executing);

        var inRange = await PostPagedListForDocAsync(new
        {
            definitionId = definition.DefinitionId,
            timestampFilters = new object[]
            {
                new { column = "CreatedAt", @operator = (int)TimestampFilterOperator.GreaterThan, timestamp = "2000-01-01T00:00:00Z" },
            },
        });
        Assert.That(GetLong(inRange.RootElement, "total"), Is.EqualTo(1));

        var outRange = await PostPagedListForDocAsync(new
        {
            definitionId = definition.DefinitionId,
            timestampFilters = new object[]
            {
                new { column = "CreatedAt", @operator = (int)TimestampFilterOperator.GreaterThan, timestamp = "2999-01-01T00:00:00Z" },
            },
        });
        Assert.That(GetLong(outRange.RootElement, "total"), Is.EqualTo(0));
    }

    [Test]
    public async Task PostPagedListAsync_WithInvalidTimestampColumn_ReturnsBadRequest()
    {
        var response = await PostPagedListAsync(new
        {
            timestampFilters = new object[]
            {
                new { column = "HackedAt", @operator = (int)TimestampFilterOperator.GreaterThan, timestamp = "2000-01-01T00:00:00Z" },
            },
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Invalid timestamp filter column"));
    }

    [Test]
    public async Task GetAsync_ExistingInstance_ReturnsDetailWithStateAndActivityRecords()
    {
        var definition = await PrepareDefinitionAsync();
        var instanceId = await SeedInstanceAsync(definition, 2, "实例_详情", WorkflowStatus.Faulted,
            correlationId: "corr_001");
        await SeedActivityRecordsAsync(instanceId,
            new ActivityInstanceExecutionRecord
            {
                ActivityId = "act_1",
                ActivityNodeId = "node_start",
                ActivityType = "Cike.Start",
                Status = ActivityStatus.Completed,
                FinishedAt = DateTime.Now,
            },
            new ActivityInstanceExecutionRecord
            {
                ActivityId = "act_2",
                ActivityNodeId = "node_approve",
                ActivityType = "Cike.Approve",
                Status = ActivityStatus.Faulted,
                Exception = new ExceptionState("System.InvalidOperationException", "审批人缺失", null, null),
            });

        var response = await CreateClient().GetAsync($"/api/v1/WorkflowInstances/{instanceId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = (await response.Content.ReadFromJsonAsync<JsonDocument>())!.RootElement;
        Assert.That(GetLong(detail, "id"), Is.EqualTo(instanceId));
        Assert.That(GetString(detail, "definitionName"), Is.EqualTo(DefinitionName));
        Assert.That(GetString(detail, "correlationId"), Is.EqualTo("corr_001"));

        var state = detail.GetProperty("workflowState");
        Assert.That(state.GetProperty("input").GetProperty("orderId").GetString(), Is.EqualTo("A-001"));

        var activities = detail.GetProperty("activityInstances").EnumerateArray().ToList();
        Assert.That(activities, Has.Count.EqualTo(2));
        var faulted = activities.Single(x => GetString(x, "activityNodeId") == "node_approve");
        Assert.That(faulted.GetProperty("status").GetInt32(), Is.EqualTo((int)ActivityStatus.Faulted));
        Assert.That(GetString(faulted.GetProperty("exception"), "message"), Is.EqualTo("审批人缺失"));
    }

    [Test]
    public async Task GetAsync_NotExistingId_ReturnsBadRequest()
    {
        var response = await CreateClient().GetAsync("/api/v1/WorkflowInstances/999999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("工作流实例不存在"));
    }

    protected static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"请求失败 [{(int)response.StatusCode}]: {await response.Content.ReadAsStringAsync()}");
    }

    protected static string GetString(JsonElement element, string propertyName)
        => element.GetProperty(propertyName).GetString()!;

    protected static long GetLong(JsonElement element, string propertyName)
    {
        var raw = element.GetProperty(propertyName);
        return raw.ValueKind == JsonValueKind.String ? long.Parse(raw.GetString()!) : raw.GetInt64();
    }
}
