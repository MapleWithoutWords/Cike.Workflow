using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Workflow.Core.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Runners.Internals.Middlewares;

public class ExceptionRunWorkflowInstanceMiddleware(
    ILogger<ExceptionRunWorkflowInstanceMiddleware> logger,
    ILocalEventBus localEventBus,
    ILocalEventContext localEventContext)
    : ILocalEventMiddleware<RunWorkflowInstanceCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.OncePerTree;

    public async Task HandleAsync(RunWorkflowInstanceCommand @event, EventHandlerDelegate next)
    {
        var context = @event.Context;
        try
        {
            await next();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Workflow instance {WorkflowInstanceId} was cancelled", context.Workflow.Id);
            context.TransitionTo(WorkflowStatus.Cancelled);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "An exception was caught from a downstream middleware component");
            var exceptionState = ExceptionState.FromException(e);
            var now = DateTime.Now;
            var activity = context.Workflow;
            var incident = new ActivityIncident(activity.Id, activity.NodeId, activity.Type, e.Message, exceptionState, now);
            context.Incidents.Add(incident);
            context.TransitionTo(WorkflowStatus.Faulted);
            context.AddExecutionLogEntry("工作流运行失败", e.Message, exceptionState);
        }
    }
}
