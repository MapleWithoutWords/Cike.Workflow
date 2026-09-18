using Cike.Core.DependencyInjection;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Runtime.ActivityInstanceExecutionRecords;

/// <inheritdoc />
public class DefaultActivityExecutionMapper() : IActivityExecutionMapper, ISingletonDependency
{
    public ActivityInstanceExecutionRecord Map(ActivityExecutionContext source)
    {
        var outputs = source.GetOutputs();
        var inputs = source.GetInputs();
        var persistableInputs = GetPersistableInputOutput(inputs);
        var persistableOutputs = GetPersistableInputOutput(outputs);
        var persistableProperties = source.Properties;
        var persistableJournalData = new Dictionary<string, object>();
        var cancellationToken = source.CancellationToken;

        var record = new ActivityInstanceExecutionRecord
        {
            Id = source.Id,
            ActivityId = source.Activity.Id,
            ActivityNodeId = source.NodeId,
            WorkflowInstanceId = source.WorkflowExecutionContext.Id,
            ActivityType = source.Activity.Type,
            ActivityName = source.Activity.Name ?? "",
            ActivityState = persistableInputs,
            Outputs = persistableOutputs,
            Properties = persistableProperties!,
            Metadata = new Dictionary<string, object>(source.Metadata),
            Payload = persistableJournalData!,
            Exception = ExceptionState.FromException(source.Exception),
            ActivityTypeVersion = source.Activity.Version,
            CreatedAt = source.CreatedAt,
            HasBookmarks = source.WorkflowExecutionContext.Bookmarks.Where(x => x.ActivityInstanceId == source.Id).Any(),
            Status = source.Status,
            AggregateFaultCount = source.AggregateFaultCount,
            FinishedAt = source.FinishedAt ?? DateTime.Now,
            SchedulingActivityExecutionId = source.SchedulingActivityExecutionId ?? 0,
            SchedulingActivityId = source.SchedulingActivityId,
            SchedulingWorkflowInstanceId = source.SchedulingWorkflowInstanceId ?? 0,
            CallStackDepth = source.CallStackDepth
        };

        return record;
    }

    private IDictionary<string, object?> GetPersistableInputOutput(IDictionary<string, object> state)
    {
        var result = new Dictionary<string, object?>();
        foreach (var stateEntry in state)
        {
            result.Add(stateEntry.Key, stateEntry.Value);
        }

        return result;
    }
}
