using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Application.Contracts.WorkflowDefinitions;

public class SaveWorkflowDefinitionDto
{
    public string OriginalStringData { get; set; } = null!;

    public WorkflowDefinitionOptionsValueObject Options { get; set; } = new();
}
