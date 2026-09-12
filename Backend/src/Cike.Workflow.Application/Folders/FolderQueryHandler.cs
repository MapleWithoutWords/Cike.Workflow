namespace Cike.Workflow.Application.Folders;

public class FolderQueryHandler(IFolderRepository folderRepository, ICacheService<FolderCacheModel> folderCacheService)
{
    [LocalEventHandler]
    public async Task GetAsync(GetFolderQuery query, CancellationToken cancellationToken = default)
    {
        var entity = await folderRepository.FindAsync(query.Id, cancellationToken)
            ?? throw new UserFriendlyException("目录不存在，请检查后重试。");

        var allFolders = await folderCacheService.GetListAsync(e => e.WorkspaceId == entity.WorkspaceId, cancellationToken);

        query.Result = entity.Adapt<FolderDetailDto>();
        query.Result.Path = entity.Adapt<FolderCacheModel>().BuildPath(allFolders)
            .Select(e => new FolderPathDto { Id = e.Id, Name = e.Name })
            .ToList();
    }
}
