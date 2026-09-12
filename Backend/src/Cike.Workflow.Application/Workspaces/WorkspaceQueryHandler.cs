namespace Cike.Workflow.Application.Workspaces;

public class WorkspaceQueryHandler(IWorkspaceRepository workspaceRepository)
{
    [LocalEventHandler]
    public async Task GetPagedAsync(GetPagedWorkspaceListQuery query, CancellationToken cancellationToken = default)
    {
        var keyword = query.Keyword;
        Expression<Func<Workspace, bool>>? predicate = null;
        if (string.IsNullOrEmpty(keyword) == false)
        {
            predicate = x => x.Name.Contains(keyword) || x.Code.Contains(keyword);
        }

        (long Total, List<Workspace> Items) paged;
        using (workspaceRepository.BeginAsNoTracking())
        {
            paged = await workspaceRepository.GetPagedListAsync(query.PageDto, predicate, cancellationToken);
        }

        query.Result = new PagedResultDto<WorkspaceItemDto>
        {
            Total = paged.Total,
            Items = paged.Items.Adapt<List<WorkspaceItemDto>>()
        };
    }

    [LocalEventHandler]
    public async Task GetAsync(GetWorkspaceQuery query, CancellationToken cancellationToken = default)
    {
        var entity = await workspaceRepository.FindAsync(query.Id, cancellationToken)
            ?? throw new UserFriendlyException("工作空间不存在，请检查后重试。");

        query.Result = entity.Adapt<WorkspaceItemDto>();
    }
}
