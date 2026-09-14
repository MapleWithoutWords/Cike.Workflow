using System.Net;
using Cike.Workflow.Service.Open.Tests.WorkflowDefinitions;

namespace Cike.Workflow.Service.Open.Tests.Folders;

/// <summary>目录移动：parentId 变更与子树跟随、移回根、防环、目标不存在/跨空间、同名拦截。</summary>
internal class FolderMoveTest : WorkflowDefinitionTestBase
{
    private Task<HttpResponseMessage> PostMoveAsync(long folderId, long parentId)
        => CreateClient().PostAsJsonAsync($"/api/v1/Folders/Move/{folderId}", new { parentId });

    private async Task<JsonElement> GetFolderDetailAsync(long folderId)
    {
        var response = await CreateClient().GetAsync($"/api/v1/Folders?folderId={folderId}");
        await EnsureSuccessAsync(response);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement;
    }

    private async Task<long> CreateFolderNamedAsync(long workspaceId, long parentId, string name)
    {
        var response = await CreateClient().PostAsJsonAsync("/api/v1/Folders", new
        {
            workspaceId,
            parentId,
            name,
        });
        await EnsureSuccessAsync(response);
        return await ReadLongAsync(response);
    }

    /// <summary>详情 Path 段的目录 Id 序列（仅祖先节点，根在前，不含自身）。</summary>
    private static List<long> GetPathIds(JsonElement detail)
        => detail.GetProperty("path").EnumerateArray().Select(x => GetLong(x, "id")).ToList();

    [Test]
    public async Task MoveAsync_ToAnotherFolder_UpdatesParentIdAndSubtreeFollows()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var folderA = await CreateFolderAsync(workspaceId);
        var child = await CreateFolderAsync(workspaceId, folderA);
        var target = await CreateFolderAsync(workspaceId);

        var response = await PostMoveAsync(folderA, target);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await GetFolderDetailAsync(folderA);
        Assert.That(GetLong(detail, "parentId"), Is.EqualTo(target));
        // Path 只含祖先节点（根在前，不含自身）
        Assert.That(GetPathIds(detail), Is.EqualTo(new List<long> { target }));

        // 子目录随父链跟随：child 仍挂在 A 下，路径整体前插 target
        var childDetail = await GetFolderDetailAsync(child);
        Assert.That(GetLong(childDetail, "parentId"), Is.EqualTo(folderA));
        Assert.That(GetPathIds(childDetail), Is.EqualTo(new List<long> { target, folderA }));

        // 目录列表端点：新父目录下可见 A，原父目录（根）下不再可见
        var inTarget = await GetFolderListAsync(workspaceId, target);
        Assert.That(inTarget.Where(x => GetInt(x, "type") == 1).Select(x => GetLong(x, "id")), Does.Contain(folderA));
        var inRoot = await GetFolderListAsync(workspaceId, 0);
        Assert.That(inRoot.Where(x => GetInt(x, "type") == 1).Select(x => GetLong(x, "id")), Does.Not.Contain(folderA));
    }

    [Test]
    public async Task MoveAsync_WithParentIdZero_MovesToRoot()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var parent = await CreateFolderAsync(workspaceId);
        var folder = await CreateFolderAsync(workspaceId, parent);

        var response = await PostMoveAsync(folder, 0);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = GetFolderDetailAsync(folder).Result;
        Assert.That(GetLong(detail, "parentId"), Is.EqualTo(0));
        Assert.That(GetPathIds(detail), Is.Empty);
    }

    [Test]
    public async Task MoveAsync_WithTargetFolderNotFound_ReturnsBadRequest()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var folder = await CreateFolderAsync(workspaceId);

        var response = await PostMoveAsync(folder, 999_999);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("目标目录"));
    }

    [Test]
    public async Task MoveAsync_WithTargetFolderInOtherWorkspace_ReturnsBadRequest()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var folder = await CreateFolderAsync(workspaceId);
        var otherWorkspaceId = await CreateWorkspaceAsync();
        var otherFolder = await CreateFolderAsync(otherWorkspaceId);

        var response = await PostMoveAsync(folder, otherFolder);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("目标目录"));
    }

    [Test]
    public async Task MoveAsync_WithTargetIsSelf_ReturnsBadRequest()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var folder = await CreateFolderAsync(workspaceId);

        var response = await PostMoveAsync(folder, folder);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("自身"));
    }

    [Test]
    public async Task MoveAsync_WithTargetIsOwnDescendant_ReturnsBadRequest()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var folderA = await CreateFolderAsync(workspaceId);
        var child = await CreateFolderAsync(workspaceId, folderA);
        var grandChild = await CreateFolderAsync(workspaceId, child);

        // 移到直接子目录与隔代子孙目录都应被拦截
        var toChild = await PostMoveAsync(folderA, child);
        Assert.That(toChild.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await toChild.Content.ReadAsStringAsync(), Does.Contain("子目录"));

        var toGrandChild = await PostMoveAsync(folderA, grandChild);
        Assert.That(toGrandChild.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await toGrandChild.Content.ReadAsStringAsync(), Does.Contain("子目录"));
    }

    [Test]
    public async Task MoveAsync_WithSameNameInTargetFolder_ReturnsBadRequest()
    {
        var workspaceId = await CreateWorkspaceAsync();
        var name = $"同名_{Guid.NewGuid():N}".Substring(0, 20);
        var parent1 = await CreateFolderAsync(workspaceId);
        await CreateFolderNamedAsync(workspaceId, parent1, name);
        var parent2 = await CreateFolderAsync(workspaceId);
        var folder = await CreateFolderNamedAsync(workspaceId, parent2, name);

        var response = await PostMoveAsync(folder, parent1);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("同名"));
    }
}
