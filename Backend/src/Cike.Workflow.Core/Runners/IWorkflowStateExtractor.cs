namespace Cike.Workflow.Core.Runners;

public interface IWorkflowStateExtractor
{
    WorkflowState Extract(WorkflowExecutionContext workflowExecutionContext);

    Task<WorkflowExecutionContext> ApplyAsync(WorkflowExecutionContext workflowExecutionContext, WorkflowState state);
}
