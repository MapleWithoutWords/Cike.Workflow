using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Application.Contracts.WorkflowInstances;

public class ActivityInstanceExecutionRecordDto : AuditedEntityDto<long>
{
    public long WorkflowInstanceId { get; set; }

    public string ActivityId { get; set; } = null!;

    public string ActivityNodeId { get; set; } = null!;

    public string ActivityType { get; set; } = null!;

    public int ActivityTypeVersion { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    public IDictionary<string, object?>? ActivityState { get; set; }

    public IDictionary<string, object>? Payload { get; set; }

    public IDictionary<string, object?>? Outputs { get; set; }

    public IDictionary<string, object>? Properties { get; set; }

    public IDictionary<string, object>? Metadata { get; set; }

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
