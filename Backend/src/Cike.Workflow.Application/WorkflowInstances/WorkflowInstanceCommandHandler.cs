namespace Cike.Workflow.Application.WorkflowInstances;

internal class WorkflowInstanceCommandHandler(IWorkflowDispatcher workflowDispatcher, IStimulusDispatcher stimulusDispatcher)
{
    [LocalEventHandler]
    public async Task RunAsync(RunWorkflowCommand command, CancellationToken cancellationToken)
    {
        await workflowDispatcher.DispatchAsync(command.Request, new DispatchWorkflowOptions(), cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task CancelAsync(CancelWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        await workflowDispatcher.DispatchAsync(command.Request, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task ResumeAsync(ResumeWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        await stimulusDispatcher.SendAsync(command.Request, cancellationToken);
    }
}
