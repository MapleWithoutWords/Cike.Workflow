using Cike.Workflow.Core.Models;

namespace Cike.Workflow.Runtime.Models;

public class RunWorkflowInstanceRequest
{
    public string? TriggerActivityId { get; set; }

    public long? BookmarkId { get; set; }

    public ActivityHandle? ActivityHandle { get; set; }

    public IDictionary<string, object>? Properties { get; set; }

    public IDictionary<string, object>? Input { get; set; }

    public IDictionary<string, object>? Variables { get; set; }

    public bool IncludeWorkflowOutput { get; set; }

    public long? SchedulingActivityExecutionId { get; set; }

    public long? SchedulingWorkflowInstanceId { get; set; }

    public int? SchedulingCallStackDepth { get; set; }

    /// <summary>
    /// Represents an empty <see cref="RunWorkflowInstanceRequest"/> object used as a default value.
    /// </summary>
    public static RunWorkflowInstanceRequest Empty => new();
}
