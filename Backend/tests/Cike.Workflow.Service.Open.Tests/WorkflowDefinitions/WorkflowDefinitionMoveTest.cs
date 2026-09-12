using System.Net;

namespace Cike.Workflow.Service.Open.Tests.WorkflowDefinitions;

/// <summary>票④：移动工作流定义到其他目录（所有版本行一起移动）。</summary>
internal class WorkflowDefinitionMoveTest : WorkflowDefinitionTestBase
{
    private Task<HttpResponseMessage> PostMoveAsync(long id, long folderId)
        => CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Move/{id}", new { folderId });

    /// <summary>在工作空间下建一个带两个版本的定义（保存 + 发布 + 再保存 → v2 草稿）。</summary>
    private async Task<(long WorkspaceId, long SourceFolderId, long TargetFolderId, string DefinitionId, long RowId)> PrepareAsync()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var sourceFolderId = await CreateFolderAsync(workspaceId);
        var targetFolderId = await CreateFolderAsync(workspaceId);
        var definitionId = $"WF_{Guid.NewGuid():N}";
        var rowId = await CreateDefinitionAsync(workspaceId, sourceFolderId, definitionId);
        return (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId);
    }

    [Test]
    public async Task MoveAsync_移动到其他目录_所有版本FolderId更新且列表在新位置可见()
    {
        var (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId) = await PrepareAsync();
        // 保存带变量的画布，验证移动不会丢失 Options（影子属性）
        var saveWithVariables = await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new
        {
            body = CreateValidCanvas("opt"),
            options = new
            {
                variables = new object[] { new { id = "var1", name = "count", typeName = "Int32", isArray = false } },
            },
        });
        await EnsureSuccessAsync(saveWithVariables);
        await EnsureSuccessAsync(await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Publish/{rowId}", new { publishedNote = "v1" }));
        var saveAgain = await CreateClient().PostAsJsonAsync($"/api/v1/WorkflowDefinitions/Save/{rowId}", new { body = CreateValidCanvas("mv2") });
        await EnsureSuccessAsync(saveAgain);
        var draftId = await ReadLongAsync(saveAgain);
        var versionIds = new List<long> { rowId, draftId };

        var response = await PostMoveAsync(rowId, targetFolderId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // 详情断言：该定义所有版本行的 FolderId 都已更新
        foreach (var versionId in versionIds)
        {
            var detail = (await GetDetailAsync(versionId)).RootElement;
            Assert.That(GetLong(detail, "folderId"), Is.EqualTo(targetFolderId));
        }

        // 移动不改内容：v1 行的 Options（变量定义）保持不变
        var v1Detail = (await GetDetailAsync(rowId)).RootElement;
        Assert.That(GetString(v1Detail, "originalStringData"), Does.Contain("opt_start"));
        Assert.That(v1Detail.GetProperty("options").GetProperty("variables").GetArrayLength(), Is.EqualTo(1));

        // 目录列表断言：新目录可见、原目录不可见
        var inTarget = await GetFolderListAsync(workspaceId, targetFolderId);
        var itemsInTarget = inTarget.Where(x => GetInt(x, "type") == 2).ToList();
        Assert.That(itemsInTarget, Has.Count.EqualTo(1));

        var inSource = await GetFolderListAsync(workspaceId, sourceFolderId);
        Assert.That(inSource.Where(x => GetInt(x, "type") == 2).ToList(), Is.Empty);
    }

    [Test]
    public async Task MoveAsync_folderId为零_移到根目录正常工作()
    {
        var (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId) = await PrepareAsync();

        var response = await PostMoveAsync(rowId, 0);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = (await GetDetailAsync(rowId)).RootElement;
        Assert.That(GetLong(detail, "folderId"), Is.EqualTo(0));

        var inRoot = await GetFolderListAsync(workspaceId, 0);
        Assert.That(inRoot.Where(x => GetInt(x, "type") == 2).ToList(), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task MoveAsync_目标目录不存在_返回400()
    {
        var (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId) = await PrepareAsync();

        var response = await PostMoveAsync(rowId, 999_999);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("目标目录"));
    }

    [Test]
    public async Task MoveAsync_目标目录不属于该工作空间_返回400()
    {
        var (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId) = await PrepareAsync();
        var otherWorkspaceId = await CreateWorkspaceAsync();
        var otherFolderId = await CreateFolderAsync(otherWorkspaceId);

        var response = await PostMoveAsync(rowId, otherFolderId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("目标目录"));
    }

    [Test]
    public async Task MoveAsync_系统内置定义_返回400()
    {
        var (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId) = await PrepareAsync();
        await SeedDefinitionAsync(definitionId, e => e.IsSystem = true);

        var response = await PostMoveAsync(rowId, targetFolderId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("系统内置"));
    }

    [Test]
    public async Task MoveAsync_只读定义_移动不被拦截()
    {
        var (workspaceId, sourceFolderId, targetFolderId, definitionId, rowId) = await PrepareAsync();
        await SeedDefinitionAsync(definitionId, e => e.IsReadonly = true);

        var response = await PostMoveAsync(rowId, targetFolderId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = (await GetDetailAsync(rowId)).RootElement;
        Assert.That(GetLong(detail, "folderId"), Is.EqualTo(targetFolderId));
    }
}
