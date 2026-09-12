using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace Cike.EntityFrameworkCore.Repositories;

public class WorkflowInstanceRepository(CikeWorkflowDbContenxt context, IWorkflowStateSerializer workflowStateSerializer, ILogger<WorkflowInstanceRepository> logger)
    : SerializedEfCoreRepository<CikeWorkflowDbContenxt, WorkflowInstance>(context), IWorkflowInstanceRepository, IScopedDependency
{
    protected override ValueTask OnSaveAsync(WorkflowInstance entity, CancellationToken cancellationToken)
    {
        DbContext.Entry(entity).Property("SerializedWorkflowState").CurrentValue = workflowStateSerializer.Serialize(entity.WorkflowState);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnLoadAsync(WorkflowInstance? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        var json = (string?)DbContext.Entry(entity).Property("SerializedWorkflowState").CurrentValue;

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
