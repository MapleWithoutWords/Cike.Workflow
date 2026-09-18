using Cike.Core.DependencyInjection;
using Cike.Workflow.Common.Versions;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Domain.Filters;
using Cike.Workflow.Runtime;

namespace Cike.Workflow.Runtime.Services;

/// <inheritdoc />
public class WorkflowCancellationService(
    IWorkflowDefinitionService workflowDefinitionService,
    IWorkflowInstanceRepository workflowInstanceStore,
    IWorkflowDispatcher dispatcher)
    : IWorkflowCancellationService, IScopedDependency
{
    /// <inheritdoc />
    public async Task<bool> CancelWorkflowAsync(long workflowInstanceId, CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowInstanceFilter
        {
            Id = workflowInstanceId
        };
        var instance = await workflowInstanceStore.FindAsync(filter, cancellationToken);

        if (instance == null)
            return false;

        await CancelWorkflows([instance], cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<int> CancelWorkflowsAsync(IEnumerable<long> workflowInstanceIds, CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowInstanceFilter
        {
            Ids = workflowInstanceIds.ToList()
        };
        var instances = await workflowInstanceStore.FindManyAsync(filter, cancellationToken);
        return await CancelWorkflows(instances.ToList(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CancelWorkflowByDefinitionVersionAsync(long definitionVersionId, CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowInstanceFilter
        {
            DefinitionVersionId = definitionVersionId,
            WorkflowMainStatus = WorkflowMainStatus.Running
        };
        var instances = (await workflowInstanceStore.FindManyAsync(filter, cancellationToken)).ToList();
        var instanceIds = instances.Select(i => i.Id).ToList();

        return await CancelWorkflowsAsync(instanceIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CancelWorkflowByDefinitionAsync(string definitionId, VersionOptions versionOptions, CancellationToken cancellationToken = default)
    {
        // Shouldn't we get possible multiple definitions here?
        var workflowDefinition = await workflowDefinitionService.FindWorkflowDefinitionAsync(definitionId, versionOptions, cancellationToken);
        if (workflowDefinition is null)
            return 0;

        return await CancelWorkflowByDefinitionVersionAsync(workflowDefinition.Id, cancellationToken);
    }

    private async Task<int> CancelWorkflows(IList<WorkflowInstance> workflowInstances, CancellationToken cancellationToken)
    {
        var tasks = workflowInstances.Where(i => i.Status.GetMainStatus() != WorkflowMainStatus.Finished)
            .Select(i => dispatcher.DispatchAsync(new DispatchCancelWorkflowRequest
            {
                WorkflowInstanceId = i.Id
            }, cancellationToken)).ToList();

        var instanceIds = workflowInstances.Select(i => i.Id).ToList();
        await CancelChildWorkflowInstances(instanceIds, cancellationToken);
        await Task.WhenAll(tasks);

        return tasks.Count;
    }

    private async Task CancelChildWorkflowInstances(IEnumerable<long> workflowInstanceIds, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<int>>();
        var workflowInstanceIdBatches = workflowInstanceIds.Chunk(50);

        foreach (var workflowInstanceIdBatch in workflowInstanceIdBatches)
        {
            WorkflowInstanceFilter filter = new()
            {
                ParentWorkflowInstanceIds = workflowInstanceIdBatch.ToList()
            };
            var childInstances = (await workflowInstanceStore.FindManyAsync(filter, cancellationToken)).ToList();

            if (childInstances.Any())
                tasks.Add(CancelWorkflows(childInstances, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}
