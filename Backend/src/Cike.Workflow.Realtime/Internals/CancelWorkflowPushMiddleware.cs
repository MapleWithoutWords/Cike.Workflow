namespace Cike.Workflow.Realtime.Internals;

/// <summary>
/// 取消命令推送中间件：取消不经过实例运行命令（取消 = 建上下文→Cancel→存状态），
/// 运行推送管线覆盖不到，由此单独补推取消终态。挂在工作流客户端发布的
/// RunCancelWorkflowCommand 上，HTTP 取消与批量取消两条入口都经过它。
/// 取消执行中实例时，中断的运行收尾也可能推一次终态，前端按类型幂等处理即可。
/// </summary>
internal class CancelWorkflowPushMiddleware(WorkflowRealtimePusher pusher)
    : ILocalEventMiddleware<RunCancelWorkflowCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public async Task HandleAsync(RunCancelWorkflowCommand @event, EventHandlerDelegate next)
    {
        await next();

        await pusher.PushAsync(new WorkflowExecutionProgressEvent(
            @event.Context.Id, WorkflowExecutionProgressType.WorkflowCanceled,
            DateTimeOffset.Now));
    }
}
