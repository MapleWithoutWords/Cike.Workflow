using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Cike.EntityFrameworkCore.Stores;

public class WorkflowDefinitionStore(CikeWorkflowDbContenxt context, IPayloadSerializer payloadSerializer, ILogger<WorkflowDefinitionStore> logger)
    : BaseStore<WorkflowDefinition>(context), IWorkflowDefinitionStore
{
    public override async Task AddRangeAsync(IEnumerable<WorkflowDefinition> entities, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
        }
        await base.AddRangeAsync(entities, cancellationToken);
    }

    private ValueTask OnSaveAsync(WorkflowDefinition entity, CancellationToken cancellationToken)
    {
        var json = payloadSerializer.Serialize(entity.Options);

        context.Entry(entity).Property("SerializedOptions").CurrentValue = json;
        return ValueTask.CompletedTask;
    }

    public override async Task<WorkflowDefinition?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await base.FindAsync(id, cancellationToken);
        await OnLoadAsync(result, cancellationToken);
        return result;
    }

    private ValueTask OnLoadAsync(WorkflowDefinition? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        var json = (string?)context.Entry(entity).Property("SerializedOptions").CurrentValue;

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
                entity.Options = payloadSerializer.Deserialize<WorkflowDefinitionOptionsValueObject>(json);
        }
        catch (Exception exp)
        {
            logger.LogError(exp, "Could not deserialize workflow definition state: {DefinitionId}. Reverting to default state", entity.DefinitionId);
        }

        return ValueTask.CompletedTask;
    }
}
