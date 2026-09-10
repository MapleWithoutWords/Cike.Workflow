namespace Cike.Workflow.Application.Workspaces;

public class WorkspaceQueryHandler(IWorkspaceStore workspaceStore)
{
    [LocalEventHandler]
    public async Task GetPagedAsync(GetPagedWorkspaceListQuery query, CancellationToken cancellationToken = default)
    {
        var keyword = query.Keyword;
        var queryable = workspaceStore.Queryable.AsNoTracking();
        if (string.IsNullOrEmpty(keyword) == false)
        {
            queryable = queryable.Where(x => x.Name.Contains(keyword) || x.Code.Contains(keyword));
        }

        var (total, items) = await workspaceStore.ToPaginationAsync(queryable, query.PageDto, cancellationToken);

        query.Result = new PagedResultDto<WorkspaceItemDto>
        {
            Total = total,
            Items = items.Adapt<List<WorkspaceItemDto>>()
        };
    }

    [LocalEventHandler]
    public async Task GetAsync(GetWorkspaceQuery query, CancellationToken cancellationToken = default)
    {
        var entity = await workspaceStore.FindAsync(query.Id, cancellationToken)
            ?? throw new UserFriendlyException("工作空间不存在，请检查后重试。");

        query.Result = entity.Adapt<WorkspaceItemDto>();
    }
}
