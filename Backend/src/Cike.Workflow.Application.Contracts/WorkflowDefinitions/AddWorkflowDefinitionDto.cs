namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class AddWorkflowDefinitionDto
{
    public long WorkspaceId { get; set; }

    public long FolderId { get; set; }

    public string? DefinitionId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public WorkflowDefinitionType Type { get; set; }

    public bool UsableAsActivity { get; set; }
}
