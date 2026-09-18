namespace Cike.Workflow.Runtime;

public interface IWorkflowClient
{
    long WorkflowInstanceId { get; }

    Task<RunWorkflowInstanceResponse> RunInstanceAsync(RunWorkflowInstanceRequest request, CancellationToken cancellationToken = default);

    Task<RunWorkflowInstanceResponse> CreateAndRunInstanceAsync(CreateAndRunWorkflowInstanceRequest request, CancellationToken cancellationToken = default);

    Task CancelAsync(CancellationToken cancellationToken = default);

    Task<bool> InstanceExistsAsync(CancellationToken cancellationToken = default);
}
