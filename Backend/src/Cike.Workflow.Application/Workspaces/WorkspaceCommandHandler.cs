namespace Cike.Workflow.Application.Workspaces;

public class WorkspaceCommandHandler(
    IWorkspaceStore workspaceStore,
    IFolderStore folderStore,
    IWorkflowDefinitionStore workflowDefinitionStore,
    IDistributedCacheClient distributedCacheClient)
{
    private const string CodeSeqKey = "cike:workflow:workspace:code:seq";

    [LocalEventHandler]
    public async Task AddAsync(AddWorkspaceCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Dto;

        if (dto.Code.IsNullOrEmpty())
        {
            dto.Code = $"WS_{await distributedCacheClient.HashIncrementAsync(CodeSeqKey, 1)}";
        }

        await ValidateDuplicateAsync(dto.Name, dto.Code, null, cancellationToken);

        var entity = dto.Adapt<Workspace>();

        await workspaceStore.AddAsync(entity, cancellationToken);
        command.Id = entity.Id;
    }

    [LocalEventHandler]
    public async Task UpdateAsync(UpdateWorkspaceCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        await ValidateDuplicateAsync(command.Dto.Name, null, command.Id, cancellationToken);

        command.Dto.Adapt(entity);

        await workspaceStore.UpdateAsync(entity, cancellationToken);
    }

    [LocalEventHandler]
    public async Task DeleteAsync(DeleteWorkspaceCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        var hasContent = await folderStore.Queryable.AsNoTracking()
            .AnyAsync(x => x.WorkspaceId == command.Id, cancellationToken)
        || await workflowDefinitionStore.Queryable.AsNoTracking()
            .AnyAsync(x => x.WorkspaceId == command.Id, cancellationToken);
        if (hasContent)
            throw new UserFriendlyException("该工作空间下存在目录或工作流，请先删除后再移除工作空间。");

        await workspaceStore.DeleteAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 校验名称/编号是否重复（新增传 excludeId=null，更新传自身 Id 排除自身；code 传 null 表示不校验编号）。
    /// </summary>
    private async Task ValidateDuplicateAsync(string name, string? code, long? excludeId, CancellationToken cancellationToken = default)
    {
        var duplicates = await workspaceStore.Queryable.AsNoTracking()
            .Where(x => (x.Name == name && (excludeId == null || x.Id != excludeId))
                        || (code != null && x.Code == code))
            .Select(x => new { x.Name, x.Code })
            .ToListAsync(cancellationToken);

        if (duplicates.Any(x => x.Name == name))
            throw new UserFriendlyException("工作空间名称已存在，请使用其他名称。");
        if (code != null && duplicates.Any(x => x.Code == code))
            throw new UserFriendlyException("工作空间编号已存在，请使用其他编号。");
    }

    private async Task<Workspace> GetEntityAsync(long id, CancellationToken cancellationToken = default)
    {
        return await workspaceStore.FindAsync(id, cancellationToken)
            ?? throw new UserFriendlyException("工作空间不存在，请检查后重试。");
    }
}
