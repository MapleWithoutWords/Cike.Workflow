namespace Cike.Workflow.EntityFrameworkCore.Tests.Repositories;

/// <summary>
/// Folder 仓储：缓存同步行为（与 Workspace 同一 CachedEfCoreRepository 模式，取核心路径覆盖）。
/// </summary>
public class FolderRepositoryTest : RepositoryTestBase
{
    private static Folder MakeFolder(string name) => new() { WorkspaceId = 1, ParentId = 0, Name = name };

    [Test]
    public async Task InsertAsync_WithNewEntity_PersistsAndSetsCache()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFolderRepository>();
            var entity = MakeFolder("folder-cache-insert");

            await repository.InsertAsync(entity);
            id = entity.Id;

            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService<FolderCacheModel>>();
            var cached = await cacheService.GetAsync(id);
            Assert.That(cached, Is.Not.Null);
            Assert.That(cached!.Name, Is.EqualTo("folder-cache-insert"));
        }

        var persisted = await WithScopeAsync(sp => sp.GetRequiredService<IFolderRepository>().FindAsync(id));
        Assert.That(persisted, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_WithExistingEntity_RemovesFromDbAndCache()
    {
        long id;
        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFolderRepository>();
            var entity = MakeFolder("folder-cache-delete");
            await repository.InsertAsync(entity);
            id = entity.Id;
        }

        using (var scope = CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IFolderRepository>();

            await repository.DeleteAsync(id);

            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService<FolderCacheModel>>();
            Assert.That(await cacheService.GetAsync(id), Is.Null);
        }

        var persisted = await WithScopeAsync(sp => sp.GetRequiredService<IFolderRepository>().FindAsync(id));
        Assert.That(persisted, Is.Null);
    }
}
