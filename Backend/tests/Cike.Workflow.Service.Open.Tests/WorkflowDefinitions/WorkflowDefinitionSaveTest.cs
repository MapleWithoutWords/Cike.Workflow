using System.Net;

namespace Cike.Workflow.Service.Open.Tests.WorkflowDefinitions;

/// <summary>票①：保存画布草稿（草稿就地更新 / 已发布生成 v+1 新草稿）。</summary>
internal class WorkflowDefinitionSaveTest : WorkflowDefinitionTestBase
{
    private async Task<(long WorkspaceId, string DefinitionId, long RowId)> PrepareAsync()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var definitionId = $"WF_{Guid.NewGuid():N}";
        var rowId = await CreateDefinitionAsync(workspaceId, 0, definitionId);
        return (workspaceId, definitionId, rowId);
    }

    private Task<HttpResponseMessage> PostSaveAsync(long id, object dto)
        => CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{id}", dto);

    [Test]
    public async Task SaveAsync_草稿状态下保存_版本号不变且画布内容更新()
    {
        var (_, definitionId, rowId) = await PrepareAsync();

        var response = await PostSaveAsync(rowId, new { body = CreateValidCanvas("v1") });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = (await GetDetailAsync(rowId)).RootElement;
        Assert.That(GetInt(detail, "version"), Is.EqualTo(1));
        Assert.That(GetBool(detail, "isLatest"), Is.True);
        Assert.That(GetBool(detail, "isPublished"), Is.False);
        Assert.That(GetString(detail, "originalStringData"), Does.Contain("v1_start"));
    }

    [Test]
    public async Task SaveAsync_草稿状态下重复保存_不产生新版本号()
    {
        var (_, definitionId, rowId) = await PrepareAsync();

        await PostSaveAsync(rowId, new { body = CreateValidCanvas("a") });
        await PostSaveAsync(rowId, new { body = CreateValidCanvas("b") });

        var versions = await GetVersionListAsync(definitionId);
        Assert.That(versions, Has.Count.EqualTo(1));
        Assert.That(GetInt(versions[0], "version"), Is.EqualTo(1));
    }

    [Test]
    public async Task SaveAsync_最新版已发布时保存_生成v2新草稿且IsLatest转移()
    {
        var (_, definitionId, rowId) = await PrepareAsync();
        await PostSaveAsync(rowId, new { body = CreateValidCanvas("v1") });
        // 构造已发布的最新行：直库打标（发布端点在票②，此处不依赖）
        await SeedDefinitionAsync(definitionId, e => e.IsPublished = true);

        var response = await PostSaveAsync(rowId, new { body = CreateValidCanvas("v2") });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var draftId = await ReadLongAsync(response);

        // 已发布行内容不变
        var published = (await GetDetailAsync(rowId)).RootElement;
        Assert.That(GetInt(published, "version"), Is.EqualTo(1));
        Assert.That(GetBool(published, "isPublished"), Is.True);
        Assert.That(GetBool(published, "isLatest"), Is.False);
        Assert.That(GetString(published, "originalStringData"), Does.Contain("v1_start"));

        // 新草稿 v2
        var draft = (await GetDetailAsync(draftId)).RootElement;
        Assert.That(GetInt(draft, "version"), Is.EqualTo(2));
        Assert.That(GetBool(draft, "isLatest"), Is.True);
        Assert.That(GetBool(draft, "isPublished"), Is.False);
        Assert.That(GetString(draft, "originalStringData"), Does.Contain("v2_start"));
        // 元数据沿用
        Assert.That(GetString(draft, "definitionId"), Is.EqualTo(definitionId));

        var versions = await GetVersionListAsync(definitionId);
        Assert.That(versions, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task SaveAsync_保存画布内容_名称等元数据不受影响()
    {
        var (_, definitionId, rowId) = await PrepareAsync();
        var before = (await GetDetailAsync(rowId)).RootElement;
        var nameBefore = GetString(before, "name");

        var response = await PostSaveAsync(rowId, new { body = CreateValidCanvas("meta") });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var after = (await GetDetailAsync(rowId)).RootElement;
        Assert.That(GetString(after, "name"), Is.EqualTo(nameBefore));
    }

    [Test]
    public async Task SaveAsync_画布包含未知活动类型_返回400()
    {
        var (_, definitionId, rowId) = await PrepareAsync();
        var canvas = new
        {
            type = "Cike.NoSuchActivity",
            id = "bad",
            activities = Array.Empty<object>(),
            connections = Array.Empty<object>(),
        };

        var response = await PostSaveAsync(rowId, new { body = canvas });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task SaveAsync_画布内容为空_返回400()
    {
        var (_, definitionId, rowId) = await PrepareAsync();

        var response = await PostSaveAsync(rowId, new { body = (object?)null });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task SaveAsync_系统内置定义_返回400()
    {
        var (_, definitionId, rowId) = await PrepareAsync();
        await SeedDefinitionAsync(definitionId, e => e.IsSystem = true);

        var response = await PostSaveAsync(rowId, new { body = CreateValidCanvas("sys") });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("系统内置"));
    }

    [Test]
    public async Task SaveAsync_只读定义_返回400()
    {
        var (_, definitionId, rowId) = await PrepareAsync();
        await SeedDefinitionAsync(definitionId, e => e.IsReadonly = true);

        var response = await PostSaveAsync(rowId, new { body = CreateValidCanvas("ro") });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("只读"));
    }
}
