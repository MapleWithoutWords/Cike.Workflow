using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.Runners;

public interface IWorkflowRunner
{
    Task<RunWorkflowResult> RunAsync(IActivity activity, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default);
    Task<RunWorkflowResult> RunAsync(WorkflowActivity workflow, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default);
    Task<RunWorkflowResult> RunAsync(WorkflowActivity workflow, WorkflowState workflowState, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default);
    Task<RunWorkflowResult> RunAsync(WorkflowGraph workflowGraph, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default);
    Task<RunWorkflowResult> RunAsync(WorkflowGraph workflowGraph, WorkflowState workflowState, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default);
    Task<RunWorkflowResult> RunAsync(WorkflowExecutionContext workflowExecutionContext);
}
