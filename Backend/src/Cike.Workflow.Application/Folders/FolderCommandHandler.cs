namespace Cike.Workflow.Application.Folders;

public class FolderCommandHandler(
    IFolderRepository folderRepository,
    IWorkspaceRepository workspaceRepository,
    IWorkflowDefinitionStore workflowDefinitionStore)
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
    public async Task DeleteAsync(DeleteFolderCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        var hasContent = await folderRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.ParentId == command.Id, cancellationToken)
        || await workflowDefinitionStore.Queryable.AsNoTracking()
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
