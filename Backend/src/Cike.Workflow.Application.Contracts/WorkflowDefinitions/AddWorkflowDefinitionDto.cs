namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class AddWorkflowDefinitionDto
{
    public long FolderId { get; set; }

    public string DefinitionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public WorkflowDefinitionType Type { get; set; }

    public bool UsableAsActivity { get; set; }
}
