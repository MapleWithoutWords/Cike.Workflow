using Cike.Workflow.Core.WorkflowGraphs;
using Cike.Workflow.Core.WorkflowGraphs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Runners.Internals;


public class WorkflowRunner(
    IServiceProvider serviceProvider,
    IWorkflowStateExtractor workflowStateExtractor,
    IWorkflowGraphBuilder workflowGraphBuilder,
    ISnowflakeIdGenerator identityGenerator,
    ILocalEventBus localEventBus,
    ILogger<WorkflowRunner> logger)
    : IWorkflowRunner, IScopedDependency
{
    /// <inheritdoc />
    public async Task<RunWorkflowResult> RunAsync(IActivity activity, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        var workflow = WorkflowActivity.FromActivity(activity);
        var workflowGraph = await workflowGraphBuilder.BuildAsync(workflow, cancellationToken);
        return await RunAsync(workflowGraph, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RunWorkflowResult> RunAsync(WorkflowGraph workflowGraph, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        // Set up a workflow execution context.
        var instanceId = options?.WorkflowInstanceId ?? identityGenerator.NextId();
        var input = options?.Input;
        var properties = options?.Properties;
        var correlationId = options?.CorrelationId;
        var triggerActivityId = options?.TriggerActivityId;
        var parentWorkflowInstanceId = options?.ParentWorkflowInstanceId;
        var workflowExecutionContext = await WorkflowExecutionContext.CreateAsync(
            serviceProvider,
            workflowGraph,
            instanceId,
            correlationId,
            parentWorkflowInstanceId,
            input,
            properties,
            null,
            triggerActivityId,
            cancellationToken);

        // Schedule the first activity.
        workflowExecutionContext.ScheduleWorkflow(
            schedulingActivityExecutionId: options?.SchedulingActivityExecutionId,
            schedulingWorkflowInstanceId: options?.SchedulingWorkflowInstanceId,
            schedulingCallStackDepth: options?.SchedulingCallStackDepth);

        return await RunAsync(workflowExecutionContext);
    }

    /// <inheritdoc />
    public async Task<RunWorkflowResult> RunAsync(WorkflowActivity workflow, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        var workflowGraph = await workflowGraphBuilder.BuildAsync(workflow, cancellationToken);
        return await RunAsync(workflowGraph, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RunWorkflowResult> RunAsync(WorkflowActivity workflow, WorkflowState workflowState, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        var workflowGraph = await workflowGraphBuilder.BuildAsync(workflow, cancellationToken);
        return await RunAsync(workflowGraph, workflowState, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RunWorkflowResult> RunAsync(WorkflowGraph workflowGraph, WorkflowState workflowState, RunWorkflowOptions? options = null, CancellationToken cancellationToken = default)
    {
        // Create a workflow execution context.
        var input = options?.Input;
        var variables = options?.Variables;
        var properties = options?.Properties;
        var correlationId = options?.CorrelationId ?? workflowState.CorrelationId;
        var triggerActivityId = options?.TriggerActivityId;
        var parentWorkflowInstanceId = options?.ParentWorkflowInstanceId;
        var workflowExecutionContext = await WorkflowExecutionContext.CreateAsync(
            serviceProvider,
            workflowGraph,
            workflowState,
            correlationId,
            parentWorkflowInstanceId,
            input,
            properties,
            null,
            triggerActivityId,
            cancellationToken);

        var bookmarkId = options?.BookmarkId;
        var activityHandle = options?.ActivityHandle;

        if (bookmarkId.HasValue)
        {
            var bookmark = workflowState.Bookmarks.FirstOrDefault(x => x.Id == bookmarkId);

            if (bookmark != null)
                workflowExecutionContext.ScheduleBookmark(bookmark);
        }
        else if (activityHandle != null)
        {
            if (activityHandle.ActivityInstanceId.HasValue)
            {
                var activityExecutionContext = workflowExecutionContext.ActivityExecutionContexts.FirstOrDefault(x => x.Id == activityHandle.ActivityInstanceId)
                                               ?? throw new("No activity execution context found with the specified ID.");
                workflowExecutionContext.ScheduleActivityExecutionContext(activityExecutionContext);
            }
            else
            {
                var activity = workflowExecutionContext.FindActivity(activityHandle);
                if (activity != null) workflowExecutionContext.ScheduleActivity(activity);
            }
        }
        else if (workflowExecutionContext.Scheduler.HasAny)
        {
            // Do nothing. The scheduler already has activities to schedule.
        }
        else
        {
            // Check if there are any interrupted activities.
            var interruptedActivityExecutionContexts = workflowExecutionContext.ActivityExecutionContexts.Where(x => x.IsExecuting).ToList();

            if (interruptedActivityExecutionContexts.Count > 0)
            {
                // Schedule the interrupted activities.
                foreach (var pendingActivityExecutionContext in interruptedActivityExecutionContexts)
                    workflowExecutionContext.ScheduleActivityExecutionContext(pendingActivityExecutionContext);
            }
            else
            {
                // Nothing was scheduled. Schedule the workflow itself.
                var vars = variables?.Select(x => new Variable(x.Key, x.Value)).ToList();
                var schedulingActivityExecutionId = options?.SchedulingActivityExecutionId;
                var schedulingWorkflowInstanceId = options?.SchedulingWorkflowInstanceId;
                var schedulingCallStackDepth = options?.SchedulingCallStackDepth;

                workflowExecutionContext.ScheduleWorkflow(
                    variables: vars,
                    schedulingActivityExecutionId: schedulingActivityExecutionId,
                    schedulingWorkflowInstanceId: schedulingWorkflowInstanceId,
                    schedulingCallStackDepth: schedulingCallStackDepth);
            }
        }

        // Set variables, if any.
        if (variables != null)
        {
            var rootContext = workflowExecutionContext.ActivityExecutionContexts.FirstOrDefault(x => x.ParentActivityExecutionContext == null);

            if (rootContext != null)
            {
                foreach (var variable in variables)
                    rootContext.SetDynamicVariable(variable.Key, variable.Value);
            }
        }

        return await RunAsync(workflowExecutionContext);
    }

    /// <inheritdoc />
    public async Task<RunWorkflowResult> RunAsync(WorkflowExecutionContext workflowExecutionContext)
    {
        var workflow = workflowExecutionContext.Workflow;
        var cancellationToken = workflowExecutionContext.CancellationToken;

        // If the status is Pending, it means the workflow is started for the first time.
        var isStarting = workflowExecutionContext.Status == WorkflowStatus.Pending;
        if (isStarting)
        {
            workflowExecutionContext.TransitionTo(WorkflowStatus.Executing);
        }

        await localEventBus.PublishAsync(new RunWorkflowInstanceCommand(workflowExecutionContext));

        var workflowState = workflowStateExtractor.Extract(workflowExecutionContext);

        var result = workflow.ResultVariable?.Get(workflowExecutionContext.MemoryRegister);
        var activityExecutionContexts = workflowExecutionContext.ActivityExecutionContexts.ToList();
        var journal = new Journal(activityExecutionContexts);
        //await commitStateHandler.CommitAsync(workflowExecutionContext, workflowState, cancellationToken);
        return new(workflowExecutionContext, workflowState, workflowExecutionContext.Workflow, result, journal);
    }
}
