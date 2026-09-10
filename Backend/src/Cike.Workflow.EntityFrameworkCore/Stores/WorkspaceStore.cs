namespace Cike.Workflow.EntityFrameworkCore.Stores;

public class WorkspaceStore(CikeWorkflowDbContenxt context, ICacheService<WorkspaceCacheModel> cacheService) : BaseStore<Workspace, WorkspaceCacheModel>(context, cacheService), IWorkspaceStore
{
}
