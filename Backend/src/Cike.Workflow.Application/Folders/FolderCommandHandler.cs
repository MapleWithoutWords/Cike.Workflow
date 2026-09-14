namespace Cike.Workflow.Application.Folders;

public class FolderCommandHandler(
    IFolderRepository folderRepository,
    IWorkspaceRepository workspaceRepository,
    IWorkflowDefinitionRepository workflowDefinitionRepository)
{
    [LocalEventHandler]
    public async Task AddAsync(AddFolderCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Dto;

        if (!await workspaceRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.Id == dto.WorkspaceId, cancellationToken))
            throw new UserFriendlyException("所属工作空间不存在，请检查后重试。");

        if (dto.ParentId != 0
            && !await folderRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.Id == dto.ParentId && x.WorkspaceId == dto.WorkspaceId, cancellationToken))
            throw new UserFriendlyException("上级目录不存在，请检查后重试。");

        await ValidateNameDuplicateAsync(dto.WorkspaceId, dto.ParentId, dto.Name, null, cancellationToken);

        var entity = dto.Adapt<Folder>();
        await folderRepository.InsertAsync(entity, cancellationToken: cancellationToken);
        command.Id = entity.Id;
    }

    [LocalEventHandler]
    public async Task UpdateAsync(UpdateFolderCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        await ValidateNameDuplicateAsync(entity.WorkspaceId, entity.ParentId, command.Dto.Name, command.Id, cancellationToken);

        command.Dto.Adapt(entity);
        await folderRepository.UpdateAsync(entity, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task MoveAsync(MoveFolderCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (command.ParentId == entity.Id)
            throw new UserFriendlyException("目录不能移动到自身，请检查后重试。");

        if (command.ParentId != 0)
        {
            // 一次加载该工作空间的父子映射：目标必须存在于同工作空间，并沿父链上溯防环
            var parentMap = await folderRepository.GetQueryable().AsNoTracking()
                .Where(x => x.WorkspaceId == entity.WorkspaceId)
                .Select(x => new { x.Id, x.ParentId })
                .ToDictionaryAsync(x => x.Id, x => x.ParentId, cancellationToken);

            if (!parentMap.TryGetValue(command.ParentId, out var cursor))
                throw new UserFriendlyException("目标目录不存在或不属于该工作空间，请检查后重试。");

            // 目标是待移动目录的子孙节点时移动会成环，逐级上溯命中即拒绝
            while (cursor != 0)
            {
                if (cursor == entity.Id)
                    throw new UserFriendlyException("目录不能移动到自身的子目录下，请检查后重试。");
                cursor = parentMap.GetValueOrDefault(cursor);
            }
        }

        await ValidateNameDuplicateAsync(entity.WorkspaceId, command.ParentId, entity.Name, entity.Id, cancellationToken);

        // 路径由 ParentId 链推导（FolderCacheModel.BuildPath），子树随父链自然跟随，只更新本节点
        entity.ParentId = command.ParentId;
        await folderRepository.UpdateAsync(entity, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task DeleteAsync(DeleteFolderCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        var hasContent = await folderRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.ParentId == command.Id, cancellationToken)
        || await workflowDefinitionRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.FolderId == command.Id, cancellationToken);
        if (hasContent)
            throw new UserFriendlyException("该目录下存在子目录或工作流，请先删除后再移除目录。");

        await folderRepository.DeleteAsync(entity, cancellationToken: cancellationToken);
    }

    private async Task ValidateNameDuplicateAsync(long workspaceId, long parentId, string name, long? excludeId, CancellationToken cancellationToken = default)
    {
        var nameExists = await folderRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.WorkspaceId == workspaceId && x.ParentId == parentId && x.Name == name
                           && (excludeId == null || x.Id != excludeId), cancellationToken);
        if (nameExists)
            throw new UserFriendlyException("同级目录下已存在同名目录，请使用其他名称。");
    }

    private async Task<Folder> GetEntityAsync(long id, CancellationToken cancellationToken = default)
    {
        return await folderRepository.FindAsync(id, cancellationToken)
            ?? throw new UserFriendlyException("目录不存在，请检查后重试。");
    }
}
