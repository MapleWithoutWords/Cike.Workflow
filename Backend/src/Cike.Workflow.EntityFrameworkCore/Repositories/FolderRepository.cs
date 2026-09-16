namespace Cike.EntityFrameworkCore.Repositories;

public class FolderRepository(CikeWorkflowDbContext context, ICacheService<FolderCacheModel> cacheService)
    : CachedEfCoreRepository<CikeWorkflowDbContext, Folder, FolderCacheModel>(context, cacheService), IFolderRepository, IScopedDependency;
