namespace Cike.Workflow.Application.WorkflowInstances.Commands;

public record CancelWorkflowCommand(DispatchCancelWorkflowRequest Request) : Command
{
}
