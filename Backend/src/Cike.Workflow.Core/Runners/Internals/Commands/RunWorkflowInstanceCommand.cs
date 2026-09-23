namespace Cike.Workflow.Core.Runners.Internals.Commands;

public record RunWorkflowInstanceCommand(WorkflowExecutionContext Context, bool IsStarting = false) : Command
{
}
