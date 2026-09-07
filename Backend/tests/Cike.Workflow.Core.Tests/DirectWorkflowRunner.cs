using Cike.UniversalId.ULong;
using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Contexts.Models;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Schedulers.Models;
using Cike.Workflow.Core.WorkflowGraphs;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests;

/// <summary>
/// 直接执行工作流的帮助类，绕过事件总线的异步 Channel 机制。
/// 复现 WorkflowInstanceRunHandler 的执行逻辑，但同步执行。
/// </summary>
public class DirectWorkflowRunner
{
    private readonly IServiceProvider _serviceProvider;

    public DirectWorkflowRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<RunWorkflowResult> RunAsync(IActivity activity, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        var workflow = WorkflowActivity.FromActivity(activity);
        return await RunAsync(workflow, options, cancellationToken);
    }

    public async Task<RunWorkflowResult> RunAsync(WorkflowActivity workflow, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        var graphBuilder = _serviceProvider.GetRequiredService<IWorkflowGraphBuilder>();
        var workflowGraph = await graphBuilder.BuildAsync(workflow, cancellationToken);

        var instanceId = options?.WorkflowInstanceId ?? _serviceProvider.GetRequiredService<ISnowflakeIdGenerator>().NextId();
        var input = options?.Input;
        var properties = options?.Properties;
        var correlationId = options?.CorrelationId;
        var triggerActivityId = options?.TriggerActivityId;
        var parentWorkflowInstanceId = options?.ParentWorkflowInstanceId;

        var workflowExecutionContext = await WorkflowExecutionContext.CreateAsync(
            _serviceProvider,
            workflowGraph,
            instanceId,
            correlationId,
            parentWorkflowInstanceId,
            input,
            properties,
            null,
            triggerActivityId,
            cancellationToken);

        workflowExecutionContext.ScheduleWorkflow(
            schedulingActivityExecutionId: options?.SchedulingActivityExecutionId,
            schedulingWorkflowInstanceId: options?.SchedulingWorkflowInstanceId,
            schedulingCallStackDepth: options?.SchedulingCallStackDepth);

        return await RunAsync(workflowExecutionContext, workflow, cancellationToken);
    }

    public async Task<RunWorkflowResult> RunAsync(WorkflowExecutionContext context, WorkflowActivity? workflow = null, CancellationToken cancellationToken = default)
    {
        workflow ??= context.Workflow;

        context.TransitionTo(WorkflowStatus.Executing);

        var scheduler = context.Scheduler;

        while (scheduler.HasAny)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            var workItem = scheduler.Take();
            await ExecuteWorkItemAsync(context, workItem, cancellationToken);
        }

        if (context.Status.GetMainStatus() == WorkflowMainStatus.Running)
            context.TransitionTo(context.ActivityExecutionContexts.All(x => x.IsCompleted) ? WorkflowStatus.Finished : WorkflowStatus.Suspended);

        var workflowStateExtractor = _serviceProvider.GetRequiredService<IWorkflowStateExtractor>();
        var workflowState = workflowStateExtractor.Extract(context);

        var result = workflow.ResultVariable?.Get(context.MemoryRegister);
        var activityExecutionContexts = context.ActivityExecutionContexts.ToList();
        var journal = new Journal(activityExecutionContexts);

        return new RunWorkflowResult(context, workflowState, workflow, result, journal);
    }

    private async Task ExecuteWorkItemAsync(WorkflowExecutionContext context, ActivityWorkItem workItem, CancellationToken cancellationToken)
    {
        var existingActivityExecutionContext = workItem.ExistingActivityExecutionContext;

        var activityExecutionContext = existingActivityExecutionContext != null
            ? context.ActivityExecutionContexts.FirstOrDefault(x => x.Id == existingActivityExecutionContext.Id)
            : null;

        if (activityExecutionContext == null)
        {
            activityExecutionContext = await context.CreateActivityExecutionContextAsync(workItem.Activity, new ActivityInvocationOptions
            {
                Owner = workItem.Owner,
                ExistingActivityExecutionContext = workItem.ExistingActivityExecutionContext,
                Variables = workItem.Variables,
                Input = workItem.Input,
                SchedulingActivityExecutionId = workItem.SchedulingActivityExecutionId,
                SchedulingWorkflowInstanceId = workItem.SchedulingWorkflowInstanceId,
                SchedulingCallStackDepth = workItem.SchedulingCallStackDepth
            });
            activityExecutionContext.Taint();
            context.AddActivityExecutionContext(activityExecutionContext);
        }

        await ExecuteActivityAsync(context, activityExecutionContext, cancellationToken);
    }

    private async Task ExecuteActivityAsync(WorkflowExecutionContext workflowExecutionContext, ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        await context.EvaluateInputPropertiesAsync();

        if (context.CancellationToken.IsCancellationRequested)
        {
            context.TransitionTo(ActivityStatus.Canceled);
            return;
        }

        if (!await context.Activity.CanExecuteAsync(context))
        {
            context.TransitionTo(ActivityStatus.Pending);
            return;
        }

        using var executionState = context.EnterExecution();
        context.TransitionTo(ActivityStatus.Running);

        if (workflowExecutionContext.ExecuteDelegate != null)
        {
            await workflowExecutionContext.ExecuteDelegate(context);
        }
        else
        {
            await context.Activity.ExecuteAsync(context);
        }

        workflowExecutionContext.ExecuteDelegate = null;

        var resumedBookmark = workflowExecutionContext.ResumedBookmarkContext?.Bookmark;
        if (resumedBookmark is { AutoBurn: true })
        {
            workflowExecutionContext.Bookmarks.Remove(resumedBookmark);
        }
    }
}
