using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Serialization;

namespace Cike.Workflow.Application.WorkflowDefinitions;

public class WorkflowDefinitionCommandHandler(
    IWorkflowDefinitionStore workflowDefinitionStore,
    IWorkspaceStore workspaceStore,
    IFolderStore folderStore,
    IDistributedCacheClient distributedCacheClient,
    IActivitySerializer activitySerializer)
{
    private const string DefinitionIdSeqKey = "cike:workflow:workflow-definition:code:seq";

    [LocalEventHandler]
    public async Task AddAsync(AddWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Dto;

        if (!await workspaceStore.Queryable.AsNoTracking()
            .AnyAsync(x => x.Id == dto.WorkspaceId, cancellationToken))
            throw new UserFriendlyException("所属工作空间不存在，请检查后重试。");

        if (dto.FolderId != 0
            && !await folderStore.Queryable.AsNoTracking()
            .AnyAsync(x => x.Id == dto.FolderId && x.WorkspaceId == dto.WorkspaceId, cancellationToken))
            throw new UserFriendlyException("目录不属于该工作空间，请检查后重试。");

        if (dto.DefinitionId.IsNullOrEmpty())
        {
            dto.DefinitionId = $"WF_{await distributedCacheClient.HashIncrementAsync(DefinitionIdSeqKey, 1)}";
        }

        await ValidateDuplicateAsync(dto.WorkspaceId, dto.Name, dto.DefinitionId, null, cancellationToken);

        var entity = dto.Adapt<WorkflowDefinition>();
        entity.OriginalStringData = activitySerializer.Serialize(new Flowchart());

        await workflowDefinitionStore.AddAsync(entity, cancellationToken);
        command.Id = entity.Id;
    }

    [LocalEventHandler]
    public async Task UpdateAsync(UpdateWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (entity.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许修改。");

        await ValidateDuplicateAsync(entity.WorkspaceId, command.Dto.Name, null, entity.DefinitionId, cancellationToken);

        command.Dto.Adapt(entity);
        await workflowDefinitionStore.UpdateAsync(entity, cancellationToken);
    }

    [LocalEventHandler]
    public async Task DeleteAsync(DeleteWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (entity.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许删除。");

        var versions = await workflowDefinitionStore.Queryable
            .Where(x => x.DefinitionId == entity.DefinitionId)
            .ToListAsync(cancellationToken);

        await workflowDefinitionStore.DeleteRangeAsync(versions, cancellationToken);
    }

    private async Task ValidateDuplicateAsync(long workspaceId, string name, string? definitionId, string? excludeDefinitionId, CancellationToken cancellationToken = default)
    {
        var duplicates = await workflowDefinitionStore.Queryable.AsNoTracking()
            .Where(x => x.IsLatest
                        && (definitionId != null && x.DefinitionId == definitionId
                            || x.WorkspaceId == workspaceId && x.Name == name
                               && (excludeDefinitionId == null || x.DefinitionId != excludeDefinitionId)))
            .Select(x => new { x.DefinitionId, x.Name })
            .ToListAsync(cancellationToken);

        if (definitionId != null && duplicates.Any(x => x.DefinitionId == definitionId))
            throw new UserFriendlyException("工作流编号已存在，请使用其他编号。");
        if (duplicates.Any(x => x.Name == name))
            throw new UserFriendlyException("工作流名称在该工作空间内已存在，请使用其他名称。");
    }

    private async Task<WorkflowDefinition> GetEntityAsync(long id, CancellationToken cancellationToken = default)
    {
        return await workflowDefinitionStore.FindAsync(id, cancellationToken)
            ?? throw new UserFriendlyException("工作流定义不存在，请检查后重试。");
    }
}
