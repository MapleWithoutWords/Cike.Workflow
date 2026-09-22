namespace Cike.Workflow.Realtime.Internals;

/// <summary>
/// 进度推送出口：统一组名、序列化载荷与异常隔离——推送失败只记日志，
/// 不向命令管线抛出，实时功能的故障不影响工作流执行结果。
/// </summary>
internal class WorkflowRealtimePusher(IHubContext<WorkflowRealtimeHub> hubContext, ILogger<WorkflowRealtimePusher> logger)
    : ITransientDependency
{
    public async Task PushAsync(WorkflowExecutionProgressEvent @event)
    {
        try
        {
            await hubContext.Clients
                .Group(WorkflowRealtimeHub.GroupName(@event.WorkflowInstanceId))
                .SendCoreAsync(WorkflowRealtimeHub.ProgressMethod, [@event], CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception,
                "Failed to push workflow execution progress for instance {WorkflowInstanceId}, type {ProgressType}",
                @event.WorkflowInstanceId, @event.Type);
        }
    }
}
