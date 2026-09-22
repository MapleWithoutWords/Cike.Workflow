namespace Cike.Workflow.Realtime.Internals;

/// <summary>
/// 取消命令推送中间件：取消不经过实例运行命令（取消 = 建上下文→Cancel→存状态），
/// 运行推送管线覆盖不到，由此单独补推取消终态。
/// 取消执行中实例时，中断的运行收尾也会推一次终态，前端按类型幂等处理即可。
/// </summary>
internal class CancelWorkflowPushMiddleware(WorkflowRealtimePusher pusher)
    : ILocalEventMiddleware<CancelWorkflowCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public async Task HandleAsync(CancelWorkflowCommand @event, EventHandlerDelegate next)
    {
        await next();

        await pusher.PushAsync(new WorkflowExecutionProgressEvent(
            @event.Request.WorkflowInstanceId, WorkflowExecutionProgressType.WorkflowCanceled,
            DateTimeOffset.Now));
    }
}
