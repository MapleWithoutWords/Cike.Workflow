using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Cike.EntityFrameworkCore.Stores;

public class WorkflowInstanceStore(
    CikeWorkflowDbContenxt context,
    IWorkflowStateSerializer workflowStateSerializer,
    ILogger<WorkflowInstanceStore> logger) : BaseStore<WorkflowInstance>(context), IWorkflowInstanceStore
{
    public override async Task AddRangeAsync(IEnumerable<WorkflowInstance> entities, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
        }
        await base.AddRangeAsync(entities, cancellationToken);
    }

    public override async Task<WorkflowInstance?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await base.FindAsync(id, cancellationToken);
        await OnLoadAsync(result, cancellationToken);
        return result;
    }

    private ValueTask OnSaveAsync(WorkflowInstance entity, CancellationToken cancellationToken)
    {
        var json = workflowStateSerializer.Serialize(entity.WorkflowState);

        context.Entry(entity).Property("SerializedWorkflowState").CurrentValue = json;
        return ValueTask.CompletedTask;
    }

    private ValueTask OnLoadAsync(WorkflowInstance? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        var json = (string?)context.Entry(entity).Property("SerializedWorkflowState").CurrentValue;

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
                entity.WorkflowState = workflowStateSerializer.Deserialize<WorkflowState>(json);
        }
        catch (Exception exp)
        {
            logger.LogError(exp, "Could not deserialize workflow instance state: {DefinitionId}. Reverting to default state", entity.DefinitionId);
        }

        return ValueTask.CompletedTask;
    }
}
