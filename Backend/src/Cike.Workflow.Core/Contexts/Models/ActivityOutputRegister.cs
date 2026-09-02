namespace Cike.Workflow.Core.Contexts.Models;

public record ActivityOutputRecord(long ContainerId, string ActivityId, long ActivityInstanceId, string OutputName, object? Value);

public class ActivityOutputRegister
{
    private readonly Dictionary<string, List<ActivityOutputRecord>> _recordsByActivityIdAndOutputName = new();
    private readonly Dictionary<string, ActivityOutputRecord> _recordsByActivityInstanceIdAndOutputName = new();

    public const string DEFAULT_OUTPUT_NAME = "Result";

    public void Record(ActivityExecutionContext activityExecutionContext, object? outputValue)
    {
        Record(activityExecutionContext, null, outputValue);
    }

    public void Record(ActivityExecutionContext activityExecutionContext, string? outputName, object? outputValue)
    {
        var activityId = activityExecutionContext.Activity.Id;
        var activityInstanceId = activityExecutionContext.Id;
        var containerId = activityExecutionContext.ParentActivityExecutionContext?.Id ?? activityExecutionContext.WorkflowExecutionContext.Id;

        outputName ??= DEFAULT_OUTPUT_NAME;

        // Inspect the output descriptor to see if the specified output name matches any PropertyInfo's name.
        // If so, use that descriptor's name instead.
        var outputDescriptor = activityExecutionContext.ActivityDescriptor.Outputs.FirstOrDefault(x => x.ClrName == outputName);

        if (outputDescriptor != null)
            outputName = outputDescriptor.Name;

        var record = new ActivityOutputRecord(containerId, activityId, activityInstanceId, outputName, outputValue);

        _recordsByActivityInstanceIdAndOutputName[CreateActivityInstanceIdLookupKey(activityInstanceId, outputName)] = record;

        var scopedRecordsKey = CreateActivityIdLookupKey(activityId, outputName);

        if (!_recordsByActivityIdAndOutputName.TryGetValue(scopedRecordsKey, out var scopedRecords))
        {
            scopedRecords = new();
            _recordsByActivityIdAndOutputName[scopedRecordsKey] = scopedRecords;
        }

        scopedRecords.Add(record);
    }

    public IEnumerable<ActivityOutputRecord> FindMany(string activityId, string? outputName = null)
    {
        var key = CreateActivityIdLookupKey(activityId, outputName);
        return _recordsByActivityIdAndOutputName.TryGetValue(key, out var records) ? records : Enumerable.Empty<ActivityOutputRecord>();
    }

    public object? FindOutputByActivityId(string activityId, string? outputName = null)
    {
        var key = CreateActivityIdLookupKey(activityId, outputName);
        return !_recordsByActivityIdAndOutputName.TryGetValue(key, out var records)
            ? null
            : records.LastOrDefault()?.Value; // Always return the last value.
    }

    public object? FindOutputByActivityInstanceId(long activityInstanceId, string? outputName = null)
    {
        var key = CreateActivityInstanceIdLookupKey(activityInstanceId, outputName);
        return !_recordsByActivityInstanceIdAndOutputName.TryGetValue(key, out var record)
            ? null
            : record.Value;
    }

    private string CreateActivityIdLookupKey(string activityId, string? outputName) => $"{activityId}:{outputName ?? DEFAULT_OUTPUT_NAME}";
    private string CreateActivityInstanceIdLookupKey(long activityInstanceId, string? outputName) => $"{activityInstanceId}:{outputName ?? DEFAULT_OUTPUT_NAME}";
}
