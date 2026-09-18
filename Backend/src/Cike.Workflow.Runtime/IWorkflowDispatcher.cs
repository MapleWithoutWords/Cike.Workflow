namespace Cike.Workflow.Runtime;

public interface IWorkflowDispatcher
{
    Task DispatchAsync(DispatchWorkflowDefinitionRequest request, DispatchWorkflowOptions? options, CancellationToken cancellationToken = default);

    Task DispatchAsync(DispatchWorkflowInstanceRequest request, DispatchWorkflowOptions? options, CancellationToken cancellationToken = default);

    Task DispatchAsync(DispatchCancelWorkflowRequest request, CancellationToken cancellationToken = default);
}
