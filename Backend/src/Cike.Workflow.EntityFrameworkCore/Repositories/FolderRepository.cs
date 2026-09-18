namespace Cike.EntityFrameworkCore.Repositories;

public class FolderRepository(CikeWorkflowDbContext context, ICacheService<FolderCacheModel> cacheService)
    : CachedEfCoreRepository<CikeWorkflowDbContext, Folder, FolderCacheModel>(context, cacheService), IFolderRepository, IScopedDependency
{
    public async Task<Dictionary<long, long>> GetParentMapAsync(long workspaceId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable().AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .Select(x => new { x.Id, x.ParentId })
            .ToDictionaryAsync(x => x.Id, x => x.ParentId, cancellationToken);
    }
}
