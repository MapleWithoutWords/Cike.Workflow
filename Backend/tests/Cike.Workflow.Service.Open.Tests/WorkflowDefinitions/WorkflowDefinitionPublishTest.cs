using System.Net;

namespace Cike.Workflow.Service.Open.Tests.WorkflowDefinitions;

/// <summary>票②：发布工作流定义（严格画布校验 + 就地打标）。</summary>
internal class WorkflowDefinitionPublishTest : WorkflowDefinitionTestBase
{
    private async Task<(string DefinitionId, long RowId)> PrepareSavedAsync(string canvasPrefix = "pub")
    {
        var workspaceId = await CreateWorkspaceAsync();
        var definitionId = $"WF_{Guid.NewGuid():N}";
        var rowId = await CreateDefinitionAsync(workspaceId, 0, definitionId);
        var save = await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new { body = CreateValidCanvas(canvasPrefix) });
        await EnsureSuccessAsync(save);
        return (definitionId, rowId);
    }

    private Task<HttpResponseMessage> PostPublishAsync(long id, string? note = null)
        => CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Publish/{id}", new { publishedNote = note });

    [Test]
    public async Task PublishAsync_合法画布_版本列表显示发布状态发布人和备注()
    {
        var (definitionId, rowId) = await PrepareSavedAsync();

        var response = await PostPublishAsync(rowId, "首个正式版本");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var versions = await GetVersionListAsync(definitionId);
        Assert.That(versions, Has.Count.EqualTo(1));
        var version = versions[0];
        Assert.That(GetBool(version, "isPublished"), Is.True);
        Assert.That(GetString(version, "publishedNote"), Is.EqualTo("首个正式版本"));
        Assert.That(GetString(version, "publishedBy"), Is.EqualTo(TestAuthHandler.TestUserId));
        Assert.That(GetDateTime(version, "publishedAt"), Is.GreaterThan(DateTime.MinValue));
    }

    [Test]
    public async Task PublishAsync_缺少开始节点_返回400且消息可定位()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var definitionId = $"WF_{Guid.NewGuid():N}";
        var rowId = await CreateDefinitionAsync(workspaceId, 0, definitionId);
        var canvas = new
        {
            type = "Cike.Flowchart",
            id = "nos_flowchart",
            activities = new object[] { new { type = "Cike.End", id = "nos_end" } },
            connections = Array.Empty<object>(),
        };
        await EnsureSuccessAsync(await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new { body = canvas }));

        var response = await PostPublishAsync(rowId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("开始节点"));
    }

    [Test]
    public async Task PublishAsync_存在孤立节点_返回400且消息可定位()
    {
        var (definitionId, rowId) = await PrepareSavedAsync("orphan");
        // 在合法画布基础上追加一个未接入流程的 WriteLine 节点
        var canvas = new
        {
            type = "Cike.Flowchart",
            id = "orphan_flowchart",
            activities = new object[]
            {
                new { type = "Cike.Start", id = "orphan_start" },
                new { type = "Cike.End", id = "orphan_end" },
                new { type = "Cike.WriteLine", id = "orphan_isolated", text = new { } },
            },
            connections = new object[]
            {
                new { source = new { activityId = "orphan_start" }, target = new { activityId = "orphan_end" } },
            },
        };
        await EnsureSuccessAsync(await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new { body = canvas }));

        var response = await PostPublishAsync(rowId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Does.Contain("孤立节点"));
        Assert.That(message, Does.Contain("orphan_isolated"));
    }

    [Test]
    public async Task PublishAsync_变量定义非法_返回400且消息可定位()
    {
        var (definitionId, rowId) = await PrepareSavedAsync("var");
        var variables = new object[]
        {
            new { id = "var1", name = "count", typeName = "Int32", isArray = false },
            new { id = "var2", name = "count", typeName = "Int32", isArray = false },
        };
        await EnsureSuccessAsync(await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new
        {
            body = CreateValidCanvas("var"),
            options = new { variables },
        }));

        var response = await PostPublishAsync(rowId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Does.Contain("变量"));
        Assert.That(message, Does.Contain("重复"));
    }

    [Test]
    public async Task PublishAsync_活动必填属性缺失_返回400且消息可定位()
    {
        var (definitionId, rowId) = await PrepareSavedAsync("req");
        // WriteLine 未设置 text → 必填属性缺失
        var canvas = new
        {
            type = "Cike.Flowchart",
            id = "req_flowchart",
            activities = new object[]
            {
                new { type = "Cike.Start", id = "req_start" },
                new { type = "Cike.WriteLine", id = "req_write" },
                new { type = "Cike.End", id = "req_end" },
            },
            connections = new object[]
            {
                new { source = new { activityId = "req_start" }, target = new { activityId = "req_write" } },
                new { source = new { activityId = "req_write" }, target = new { activityId = "req_end" } },
            },
        };
        await EnsureSuccessAsync(await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new { body = canvas }));

        var response = await PostPublishAsync(rowId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Does.Contain("必填属性"));
        Assert.That(message, Does.Contain("req_write"));
    }

    [Test]
    public async Task PublishAsync_最新行已是发布态_返回400()
    {
        var (definitionId, rowId) = await PrepareSavedAsync("dup");
        await EnsureSuccessAsync(await PostPublishAsync(rowId));

        var response = await PostPublishAsync(rowId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("新草稿"));
    }

    [Test]
    public async Task PublishAsync_系统内置定义_返回400()
    {
        var (definitionId, rowId) = await PrepareSavedAsync("sys");
        await SeedDefinitionAsync(definitionId, e => e.IsSystem = true);

        var response = await PostPublishAsync(rowId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("系统内置"));
    }

    [Test]
    public async Task PublishAsync_后续发布_历史版本发布标记保留()
    {
        var (definitionId, rowId) = await PrepareSavedAsync("hist");
        await EnsureSuccessAsync(await PostPublishAsync(rowId, "v1"));

        // 发布后再次保存 → v2 草稿 → 再发布
        var save = await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new { body = CreateValidCanvas("hist2") });
        await EnsureSuccessAsync(save);
        var draftId = await ReadLongAsync(save);
        await EnsureSuccessAsync(await PostPublishAsync(draftId, "v2"));

        var versions = await GetVersionListAsync(definitionId);
        Assert.That(versions, Has.Count.EqualTo(2));
        Assert.That(versions.All(x => GetBool(x, "isPublished")), Is.True);
        Assert.That(GetString(versions[0], "publishedNote"), Is.EqualTo("v2"));
        Assert.That(GetString(versions[1], "publishedNote"), Is.EqualTo("v1"));
    }
}
