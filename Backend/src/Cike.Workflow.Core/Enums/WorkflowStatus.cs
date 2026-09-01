using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Enums;

public enum WorkflowStatus
{
    Pending,

    Executing,

    Suspended,

    Finished,

    Cancelled,

    Faulted,

    Interrupted,
}

public static class WorkflowStatusExtensions
{
    public static WorkflowMainStatus GetMainStatus(this WorkflowStatus status) =>
        status switch
        {
            WorkflowStatus.Pending => WorkflowMainStatus.Running,
            WorkflowStatus.Cancelled => WorkflowMainStatus.Finished,
            WorkflowStatus.Executing => WorkflowMainStatus.Running,
            WorkflowStatus.Faulted => WorkflowMainStatus.Finished,
            WorkflowStatus.Finished => WorkflowMainStatus.Finished,
            WorkflowStatus.Suspended => WorkflowMainStatus.Running,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    public static bool IsFinished(this WorkflowStatus status) =>
        status.GetMainStatus() == WorkflowMainStatus.Finished;
}
