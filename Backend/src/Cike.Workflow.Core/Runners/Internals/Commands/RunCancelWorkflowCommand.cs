namespace Cike.Workflow.Core.Runners.Internals.Commands;

public record RunCancelWorkflowCommand(WorkflowExecutionContext Context) : Command
{
}
