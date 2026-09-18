using Cike.Core.DependencyInjection;

namespace Cike.Workflow.Runtime.Triggers.Internals;

/// <inheritdoc />
public class TriggerBoundWorkflowService(ITriggerRepository triggerRepository, IWorkflowDefinitionService workflowDefinitionService, ILogger<TriggerBoundWorkflowService> logger) : ITriggerBoundWorkflowService, IScopedDependency
{
    /// <inheritdoc />
    public async Task<IEnumerable<TriggerBoundWorkflow>> FindManyAsync(string activityTypeName, object stimulus, CancellationToken cancellationToken = default)
    {
        var triggers = await triggerRepository.FindTriggersAsync(activityTypeName, stimulus, cancellationToken);
        return await FindManyAsync(triggers, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TriggerBoundWorkflow>> FindManyAsync(string stimulusHash, CancellationToken cancellationToken = default)
    {
        var triggers = await triggerRepository.FindTriggersAsync(stimulusHash, cancellationToken);
        return await FindManyAsync(triggers, cancellationToken);
    }

    private async Task<IEnumerable<TriggerBoundWorkflow>> FindManyAsync(IEnumerable<TriggerEntity> triggers, CancellationToken cancellationToken = default)
    {
        var groupedTriggers = triggers.GroupBy(x => x.WorkflowDefinitionVersionId);
        var triggerBoundWorkflows = new List<TriggerBoundWorkflow>();

        foreach (var triggerGroup in groupedTriggers)
        {
            var workflowDefinitionVersionId = triggerGroup.Key;
            var workflowGraph = await workflowDefinitionService.FindWorkflowGraphAsync(workflowDefinitionVersionId, cancellationToken);

            if (workflowGraph == null)
            {
                logger.LogWarning("Workflow definition with ID {WorkflowDefinitionVersionId} not found", workflowDefinitionVersionId);
                continue;
            }

            var triggerBoundWorkflow = new TriggerBoundWorkflow(workflowGraph, triggerGroup.ToList());
            triggerBoundWorkflows.Add(triggerBoundWorkflow);
        }

        return triggerBoundWorkflows;
    }
}
