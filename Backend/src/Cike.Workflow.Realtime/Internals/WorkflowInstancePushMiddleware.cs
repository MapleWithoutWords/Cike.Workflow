namespace Cike.Workflow.Realtime.Internals;

/// <summary>
/// 实例运行命令推送中间件：命令收尾后按工作流状态推送实例终态（完成 / 挂起 / 失败 / 取消）。
/// 异常由外层的异常中间件转换成状态，本中间件统一走状态映射，不做 catch。
/// </summary>
internal class WorkflowInstancePushMiddleware(WorkflowRealtimePusher pusher)
    : ILocalEventMiddleware<RunWorkflowInstanceCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public async Task HandleAsync(RunWorkflowInstanceCommand @event, EventHandlerDelegate next)
    {
        var context = @event.Context;
        try
        {
            await next();
        }
        catch
        {
            // 异常经本中间件后由外层异常中间件转换成状态并吞掉；活动故障路径此刻
            // 状态已是 Faulted（活动侧 errorHandler 所置），按当下状态如实补推终态
            var type = ResolveTerminalType(context.Status);
            if (type.HasValue)
            {
                await pusher.PushAsync(new WorkflowExecutionProgressEvent(
                    context.Id, type.Value, DateTimeOffset.Now));
            }
            throw;
        }

        var terminalType = ResolveTerminalType(context.Status);
        if (terminalType.HasValue)
        {
            await pusher.PushAsync(new WorkflowExecutionProgressEvent(
                context.Id, terminalType.Value, DateTimeOffset.Now));
        }
    }

    /// <summary>状态 → 终态推送类型；非终态（执行中 / 中断）不推送。</summary>
    private static WorkflowExecutionProgressType? ResolveTerminalType(WorkflowStatus status) => status switch
    {
        WorkflowStatus.Finished => WorkflowExecutionProgressType.WorkflowFinished,
        WorkflowStatus.Suspended => WorkflowExecutionProgressType.WorkflowSuspended,
        WorkflowStatus.Cancelled => WorkflowExecutionProgressType.WorkflowCanceled,
        WorkflowStatus.Faulted => WorkflowExecutionProgressType.WorkflowFaulted,
        _ => null,
    };
}
