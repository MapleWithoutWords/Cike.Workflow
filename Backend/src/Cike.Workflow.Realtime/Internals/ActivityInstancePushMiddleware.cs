using Cike.Workflow.Core.Contexts;

namespace Cike.Workflow.Realtime.Internals;

/// <summary>
/// 节点运行命令推送中间件：next() 前推节点开始，next() 后按完成状态推节点完成，
/// 异常时推节点失败并重抛（异常语义交给外层异常中间件处理，这里只负责如实上报）。
/// </summary>
internal class ActivityInstancePushMiddleware(WorkflowRealtimePusher pusher)
    : ILocalEventMiddleware<RunActivityInstanceCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public async Task HandleAsync(RunActivityInstanceCommand @event, EventHandlerDelegate next)
    {
        var context = @event.Context;
        var workflowInstanceId = context.WorkflowExecutionContext.Id;

        await pusher.PushAsync(BuildEvent(workflowInstanceId, context, WorkflowExecutionProgressType.ActivityStarted));

        try
        {
            await next();

            if (context.Status == ActivityStatus.Completed)
            {
                await pusher.PushAsync(BuildEvent(workflowInstanceId, context, WorkflowExecutionProgressType.ActivityCompleted));
            }
        }
        catch
        {
            await pusher.PushAsync(BuildEvent(workflowInstanceId, context, WorkflowExecutionProgressType.ActivityFaulted));
            throw;
        }
    }

    private static WorkflowExecutionProgressEvent BuildEvent(
        long workflowInstanceId, ActivityExecutionContext context, WorkflowExecutionProgressType type)
        => new(workflowInstanceId, type, DateTimeOffset.Now, context.ActivityNode.NodeId, context.Id);
}
