namespace Cike.Workflow.Application.WorkflowDefinitions.Queries;

public record GetWorkflowDefinitionFolderListQuery(long WorkspaceId, long FolderId, string? Keyword, string Sorting = "CreatedAt desc") : Query<List<WorkflowDefinitionFolderItemDto>>
{
}
