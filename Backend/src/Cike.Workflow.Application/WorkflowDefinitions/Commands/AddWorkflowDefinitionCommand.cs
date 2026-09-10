namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

public record AddWorkflowDefinitionCommand(AddWorkflowDefinitionDto Dto) : Command
{
    public long Id { get; set; }
}
