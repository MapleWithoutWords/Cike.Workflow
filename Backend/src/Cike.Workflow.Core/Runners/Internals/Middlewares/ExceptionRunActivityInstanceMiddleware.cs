using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Workflow.Common.IncidentStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Runners.Internals.Middlewares;

internal class ExceptionRunActivityInstanceMiddleware(
    ILogger<ExceptionRunActivityInstanceMiddleware> logger,
    ILocalEventBus localEventBus,
    ILocalEventContext localEventContext)
    : ILocalEventMiddleware<RunActivityInstanceCommand>
{
    public MiddlewareExecutionPolicy ExecutionPolicy => MiddlewareExecutionPolicy.Always;

    public const string FAILED_RETRY_INTERVAL_PROPERTY = "FailedRetryInterval";
    public const string FAILED_RETRY_COUNT_PROPERTY = "FailedRetryCount";

    public async Task HandleAsync(RunActivityInstanceCommand @event, EventHandlerDelegate next)
    {
        var context = @event.Context;

        var failStategy = ResolveFailStategyAsync(context);

        await FailedStrategyHelper.ExecuteAsync(async () =>
          {
              await next();
          }, ex =>
          {
              logger.LogWarning(ex, "An exception was caught from a downstream middleware component");
              context.Fault(ex);
              var workflowExecutionContext = context.WorkflowExecutionContext;

              if (!workflowExecutionContext.Status.IsFinished())
                  workflowExecutionContext.TransitionTo(WorkflowStatus.Faulted);
          }, failStategy.Interval, failStategy.RetryCount);
    }

    private static (int Interval, int RetryCount) ResolveFailStategyAsync(ActivityExecutionContext context)
    {
        var retryInterval = 0;
        var retryCount = 1;
        if (context.Properties.TryGetValue<int>(FAILED_RETRY_INTERVAL_PROPERTY, out var outRetryInterval))
        {
            retryInterval = outRetryInterval;
        }
        else if (context.WorkflowExecutionContext.Properties.TryGetValue<int>(FAILED_RETRY_INTERVAL_PROPERTY, out outRetryInterval))
        {
            retryInterval = outRetryInterval;
        }

        if (context.Properties.TryGetValue<int>(FAILED_RETRY_COUNT_PROPERTY, out var outRetryCount))
        {
            retryCount = outRetryCount;
        }
        else if (context.WorkflowExecutionContext.Properties.TryGetValue<int>(FAILED_RETRY_COUNT_PROPERTY, out outRetryCount))
        {
            retryCount = outRetryCount;
        }

        return (retryInterval, retryCount);
    }
}
