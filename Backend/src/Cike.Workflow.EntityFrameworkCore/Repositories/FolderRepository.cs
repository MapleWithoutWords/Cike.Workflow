namespace Cike.EntityFrameworkCore.Repositories;

public class FolderRepository(CikeWorkflowDbContenxt context, ICacheService<FolderCacheModel> cacheService)
    : CachedEfCoreRepository<CikeWorkflowDbContenxt, Folder, FolderCacheModel>(context, cacheService), IFolderRepository, IScopedDependency;
