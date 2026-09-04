using Cike.Workflow.Core.Contexts.Models;

namespace Cike.Workflow.Core.Runners.Models;

/// <summary>
/// Contains information about a workflow run, such as <see cref="WorkflowState"/>.
/// </summary>
public record RunWorkflowResult(WorkflowExecutionContext WorkflowExecutionContext, WorkflowState WorkflowState, WorkflowActivity Workflow, object? Result, Journal Journal);

/// <summary>
/// Contains information about a workflow run, such as <see cref="WorkflowState"/>.
/// </summary>
public record RunWorkflowResult<TResult>(WorkflowExecutionContext WorkflowExecutionContext, WorkflowState WorkflowState, WorkflowActivity Workflow, TResult Result, Journal Journal);

public record Journal(ICollection<ActivityExecutionContext> ActivityExecutionContexts)
{
    public static Journal Empty => new([]);
}
