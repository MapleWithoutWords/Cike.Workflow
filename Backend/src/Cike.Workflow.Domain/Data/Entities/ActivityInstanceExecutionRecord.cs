using Cike.Data;
using Cike.Domain.Entities;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Domain.Data.Entities;

public class ActivityInstanceExecutionRecord : AuditedEntity<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public long WorkflowInstanceId { get; set; }

    public string ActivityId { get; set; } = null!;

    public string ActivityNodeId { get; set; } = null!;

    public string ActivityType { get; set; } = null!;

    public int ActivityTypeVersion { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    /// <summary>
    /// The state of the activity at the time this record is created or last updated.
    /// </summary>
    public IDictionary<string, object?>? ActivityState { get; set; }

    /// <summary>
    /// Any additional payload associated with the log record.
    /// </summary>
    public IDictionary<string, object>? Payload { get; set; }

    /// <summary>
    /// Any outputs provided by the activity.
    /// </summary>
    public IDictionary<string, object?>? Outputs { get; set; }

    /// <summary>
    /// Any properties provided by the activity.
    /// </summary>
    public IDictionary<string, object>? Properties { get; set; }

    /// <summary>
    /// Lightweight metadata associated with the activity execution.
    /// This information will be retained as part of the activity execution summary record.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during the activity execution.
    /// </summary>
    public ExceptionState? Exception { get; set; }

    public bool HasBookmarks { get; set; }

    public ActivityStatus Status { get; set; }

    public int AggregateFaultCount { get; set; }

    public DateTime FinishedAt { get; set; }

    public long SchedulingActivityExecutionId { get; set; }

    public string? SchedulingActivityId { get; set; }

    public long SchedulingWorkflowInstanceId { get; set; }

    public int? CallStackDepth { get; set; }
}
