namespace Cike.Workflow.Runtime;

public interface IStimulusDispatcher
{
    Task SendAsync(DispatchStimulusRequest request, CancellationToken cancellationToken = default);
}
