namespace Cike.Workflow.Application.Workspaces.Queries;

public record GetWorkspaceQuery(long Id) : Query<WorkspaceItemDto>;
