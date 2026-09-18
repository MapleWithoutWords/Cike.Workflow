using Cike.EventBus.Local;

namespace Cike.Workflow.Runtime.Internals.Commands;

public record DispatcherCancelWorkflowCommand(DispatchCancelWorkflowRequest Request) : BackgroundEvent
{
}
