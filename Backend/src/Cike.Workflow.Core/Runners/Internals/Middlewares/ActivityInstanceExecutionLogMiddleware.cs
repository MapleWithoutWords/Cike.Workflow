using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.LocalEventMiddlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Runners.Internals.Middlewares;

internal class ActivityInstanceExecutionLogMiddleware : ILocalEventMiddleware<RunActivityInstanceCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public async Task HandleAsync(RunActivityInstanceCommand @event, EventHandlerDelegate next)
    {
        var context = @event.Context;
        context.AddExecutionLogEntry(IsActivityBookmarked(context) ? "Resumed" : "Started");

        try
        {
            await next();

            if (context.Status == ActivityStatus.Running)
            {
                if (IsActivityBookmarked(context))
                    context.AddExecutionLogEntry("Suspended");
            }
        }
        catch (Exception exception)
        {
            context.AddExecutionLogEntry("Faulted",
                message: exception.Message,
                payload: new
                {
                    Exception = exception.GetType().FullName,
                    exception.Message,
                    exception.Source,
                    exception.Data,
                    exception.StackTrace,
                    InnerException = exception.InnerException?.GetType().FullName,
                });

            throw;
        }
    }

    private static bool IsActivityBookmarked(ActivityExecutionContext context)
    {
        return context.WorkflowExecutionContext.Bookmarks.Any(b => b.ActivityNodeId.Equals(context.ActivityNode.NodeId));
    }
}
