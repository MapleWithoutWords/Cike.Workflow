namespace Cike.Workflow.Realtime.Models;

/// <summary>
/// 执行进度推送类型：节点级四种（开始 / 完成 / 挂起 / 失败）与实例终态四种（完成 / 挂起 / 失败 / 取消）。
/// </summary>
public enum WorkflowExecutionProgressType
{
    ActivityStarted,

    ActivityCompleted,

    ActivitySuspended,

    ActivityFaulted,

    WorkflowFinished,

    WorkflowSuspended,

    WorkflowFaulted,

    WorkflowCanceled,
}

/// <summary>
/// 工作流执行进度推送载荷：经 SignalR 以 ExecutionProgress 方法推往 instance:{id} 组。
/// </summary>
public sealed record WorkflowExecutionProgressEvent(
    long WorkflowInstanceId,
    WorkflowExecutionProgressType Type,
    DateTimeOffset Timestamp,
    string? ActivityNodeId = null,
    long? ActivityInstanceId = null);
