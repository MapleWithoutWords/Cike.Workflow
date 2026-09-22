namespace Cike.Workflow.Realtime;

/// <summary>
/// 工作流实时推送 Hub：前端按实例 Watch / Unwatch，推送不由 Hub 发出，
/// 而由命令中间件经 IHubContext 发往实例组——Hub 只负责组管理。
/// </summary>
public class WorkflowRealtimeHub : Hub
{
    /// <summary>客户端回调方法名：进度事件经此方法推送给订阅者。</summary>
    public const string ProgressMethod = "ExecutionProgress";

    /// <summary>Hub 映射路径（模块初始化时 MapHub）。</summary>
    public const string Path = "/realtime/workflow";

    /// <summary>订阅指定实例的执行进度。</summary>
    public Task Watch(long workflowInstanceId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(workflowInstanceId));

    /// <summary>退订指定实例的执行进度。</summary>
    public Task Unwatch(long workflowInstanceId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(workflowInstanceId));

    /// <summary>实例组名：每个运行中实例一个组，实例 Id 是天然分隔。</summary>
    internal static string GroupName(long workflowInstanceId) => $"instance:{workflowInstanceId}";
}
