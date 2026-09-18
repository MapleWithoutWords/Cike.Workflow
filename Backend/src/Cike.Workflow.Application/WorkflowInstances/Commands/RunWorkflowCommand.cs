using Cike.Workflow.Runtime.Models;

namespace Cike.Workflow.Application.WorkflowInstances.Commands;

public record RunWorkflowCommand(DispatchWorkflowDefinitionRequest Request) : Command
{
}
