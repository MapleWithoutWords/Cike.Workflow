namespace Cike.Workflow.Application.WorkflowDefinitions.Commands;

public record UpdateWorkflowDefinitionCommand(long Id, UpdateWorkflowDefinitionDto Dto) : Command;
