namespace Cike.Workflow.Runtime.Triggers;

public interface ITriggerBoundWorkflowService
{
    /// <summary>
    /// Finds trigger-bound workflows by activity type name and stimulus.
    /// </summary>
    Task<IEnumerable<TriggerBoundWorkflow>> FindManyAsync(string activityTypeName, object stimulus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds trigger-bound workflows by stimulus hash.
    /// </summary>
    Task<IEnumerable<TriggerBoundWorkflow>> FindManyAsync(string stimulusHash, CancellationToken cancellationToken = default);
}
