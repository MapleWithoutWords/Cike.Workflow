namespace Cike.EntityFrameworkCore.Repositories;

/// <summary>
/// 自定义仓储：约定注册先于默认仓储的 TryAdd 注册，天然覆盖同实体的默认仓储。
/// </summary>
public class WorkspaceRepository(CikeWorkflowDbContenxt context, ICacheService<WorkspaceCacheModel> cacheService)
    : CachedEfCoreRepository<CikeWorkflowDbContenxt, Workspace, WorkspaceCacheModel>(context, cacheService), IWorkspaceRepository, IScopedDependency;
