namespace Cike.Workflow.Core.Models;

/// <summary>
/// Represents a handle to an activity.
/// </summary>
public class ActivityHandle
{
    public static ActivityHandle FromActivityId(string activityId) => new()
    {
        ActivityId = activityId
    };

    public static ActivityHandle FromActivityNodeId(string activityNodeId) => new()
    {
        ActivityNodeId = activityNodeId
    };

    public static ActivityHandle FromActivityInstanceId(long activityInstanceId) => new()
    {
        ActivityInstanceId = activityInstanceId
    };

    public string? ActivityId { get; init; }
    public string? ActivityNodeId { get; init; }
    public long? ActivityInstanceId { get; init; }

    public override string ToString()
    {
        return ActivityId ?? (ActivityNodeId ?? (ActivityInstanceId.HasValue ? ActivityInstanceId.ToString() : "") ?? "");
    }
}
