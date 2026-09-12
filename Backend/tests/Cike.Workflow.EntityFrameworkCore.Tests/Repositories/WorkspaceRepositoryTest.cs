using Cike.Contracts.EntityDtos;

namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// Workspace 仓储：CRUD 行为 + 缓存同步（写 set / 删 remove）。
/// </summary>
public class WorkspaceRepositoryTest : RepositoryTestBase
{
    private static Workspace MakeWorkspace(string name) => new() { Code = $"WS_{name}", Name = name };

    [Test]
    public async Task InsertAsync_WithNewEntity_PersistsAndSetsCache()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            var entity = MakeWorkspace("cache-insert");

            await repository.InsertAsync(entity);

            Assert.That(entity.Id, Is.Not.EqualTo(0));
            id = entity.Id;

            // 缓存同步：写库成功后 set
            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService<WorkspaceCacheModel>>();
            var cached = await cacheService.GetAsync(id);
            Assert.That(cached, Is.Not.Null);
            Assert.That(cached!.Name, Is.EqualTo("cache-insert"));
        }

        // 跨 Scope 可见：新 Scope 查库能查到
        var persisted = await WithScopeAsync(sp => sp.GetRequiredService<IWorkspaceRepository>().FindAsync(id));
        Assert.That(persisted, Is.Not.Null);
        Assert.That(persisted!.Name, Is.EqualTo("cache-insert"));
    }

    [Test]
    public async Task UpdateAsync_WithModifiedEntity_UpdatesCache()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            var entity = MakeWorkspace("cache-update");
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            var entity = await repository.FindAsync(id);

            entity!.Name = "cache-updated";

            await repository.UpdateAsync(entity);

            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService<WorkspaceCacheModel>>();
            var cached = await cacheService.GetAsync(id);
            Assert.That(cached, Is.Not.Null);
            Assert.That(cached!.Name, Is.EqualTo("cache-updated"));
        }
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_RemovesFromDbAndCache()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            var entity = MakeWorkspace("cache-delete");
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();

            // 按主键删除（幂等路径），同时覆盖 DeleteAsync(id) → DeleteAsync(entity) 的委托链
            await repository.DeleteAsync(id);

            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService<WorkspaceCacheModel>>();
            Assert.That(await cacheService.GetAsync(id), Is.Null);
        }

        var persisted = await WithScopeAsync(sp => sp.GetRequiredService<IWorkspaceRepository>().FindAsync(id));
        Assert.That(persisted, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithUnknownId_ReturnsSilently()
    {
        await WithScopeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IWorkspaceRepository>();

            Assert.DoesNotThrowAsync(async () => await repository.DeleteAsync(999_999_999));
            return true;
        });
    }

    [Test]
    public async Task GetPagedListAsync_WithPredicate_ReturnsCorrectPage()
    {
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
            await repository.InsertAsync(MakeWorkspace("page-1"));
            await repository.InsertAsync(MakeWorkspace("page-2"));
            await repository.InsertAsync(MakeWorkspace("unrelated"));
        }

        var paged = await WithScopeAsync(sp => sp.GetRequiredService<IWorkspaceRepository>()
            .GetPagedListAsync(new PagedAndSortedResultRequest { Page = 1, PageSize = 2 }, e => e.Code.StartsWith("WS_page-")));

        Assert.That(paged.Total, Is.EqualTo(2));
        Assert.That(paged.Items, Has.Count.EqualTo(2));
    }
}
