using Cike.Workflow.Core.Contexts;

namespace Cike.Workflow.Realtime.Internals;

/// <summary>
/// 节点运行命令推送中间件：next() 前推节点开始，next() 后按收尾状态推送——
/// 完成推 Completed；仍在运行且书签挂在本节点推 Suspended（引擎无节点挂起状态，
/// 挂起即"Running + 存在书签"，与活动执行日志中间件同款判定）；异常时推 Faulted 并重抛
/// （异常语义交给外层异常中间件处理，这里只负责如实上报）。
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
            else if (context.Status == ActivityStatus.Running && IsBookmarked(context))
            {
                await pusher.PushAsync(BuildEvent(workflowInstanceId, context, WorkflowExecutionProgressType.ActivitySuspended));
            }
        }
        catch
        {
            await pusher.PushAsync(BuildEvent(workflowInstanceId, context, WorkflowExecutionProgressType.ActivityFaulted));
            throw;
        }
    }

    /// <summary>节点挂起判定：书签挂在当前节点上（与活动执行日志中间件的判定一致）。</summary>
    private static bool IsBookmarked(ActivityExecutionContext context)
        => context.WorkflowExecutionContext.Bookmarks.Any(b => b.ActivityNodeId == context.ActivityNode.NodeId);

    private static WorkflowExecutionProgressEvent BuildEvent(
        long workflowInstanceId, ActivityExecutionContext context, WorkflowExecutionProgressType type)
        => new(workflowInstanceId, type, DateTimeOffset.Now, context.ActivityNode.NodeId, context.Id);
}
