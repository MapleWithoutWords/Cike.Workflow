namespace Cike.EntityFrameworkCore.Repositories;

/// <summary>
/// 自定义仓储：约定注册先于默认仓储的 TryAdd 注册，天然覆盖同实体的默认仓储。
/// </summary>
public class WorkspaceRepository(CikeWorkflowDbContext context, ICacheService<WorkspaceCacheModel> cacheService)
    : CachedEfCoreRepository<CikeWorkflowDbContext, Workspace, WorkspaceCacheModel>(context, cacheService), IWorkspaceRepository, IScopedDependency;
