namespace Cike.Workflow.Runtime;

public interface IWorkflowRuntime
{
    ValueTask<IWorkflowClient> CreateClientAsync(long? workflowInstanceId, CancellationToken cancellationToken = default);
}
