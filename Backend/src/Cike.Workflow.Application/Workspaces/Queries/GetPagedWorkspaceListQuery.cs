namespace Cike.Workflow.Application.Workspaces.Queries;

public record GetPagedWorkspaceListQuery(string? Keyword, PagedAndSortedResultRequest PageDto)
    : Query<PagedResultDto<WorkspaceItemDto>>;
