using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Core.Contexts.Models;

public class ActivityIncident
{
    [JsonConstructor]
    public ActivityIncident()
    {
    }

    public ActivityIncident(string activityId, string activityNodeId, string activityType, string message, ExceptionState? exception, DateTime timestamp, long? activityInstanceId = null)
    {
        ActivityId = activityId;
        ActivityNodeId = activityNodeId;
        ActivityType = activityType;
        Message = message;
        Exception = exception;
        Timestamp = timestamp;
        ActivityInstanceId = activityInstanceId;
    }

    public string ActivityId { get; init; } = default!;

    public string ActivityNodeId { get; init; } = default!;

    public long? ActivityInstanceId { get; init; }

    public string ActivityType { get; init; } = default!;

    public string Message { get; init; } = default!;

    public ExceptionState? Exception { get; init; }

    public DateTime Timestamp { get; init; }
}
