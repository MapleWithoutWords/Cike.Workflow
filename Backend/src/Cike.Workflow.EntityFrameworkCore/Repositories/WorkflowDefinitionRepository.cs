using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace Cike.EntityFrameworkCore.Repositories;

public class WorkflowDefinitionRepository(CikeWorkflowDbContenxt context, IPayloadSerializer payloadSerializer, ILogger<WorkflowDefinitionRepository> logger)
    : SerializedEfCoreRepository<CikeWorkflowDbContenxt, WorkflowDefinition>(context), IWorkflowDefinitionRepository, IScopedDependency
{
    /// <summary>与迁移前一致：列表查询不反序列化影子属性，仅排序。</summary>
    public async Task<List<WorkflowDefinition>> GetListAsync(Expression<Func<WorkflowDefinition, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default)
    {
        return await GetQueryable().AsNoTracking().Where(filter).OrderBy(sorting).ToListAsync(cancellationToken);
    }

    protected override ValueTask OnSaveAsync(WorkflowDefinition entity, CancellationToken cancellationToken)
    {
        DbContext.Entry(entity).Property("SerializedOptions").CurrentValue = payloadSerializer.Serialize(entity.Options);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnLoadAsync(WorkflowDefinition? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        var json = (string?)DbContext.Entry(entity).Property("SerializedOptions").CurrentValue;

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
