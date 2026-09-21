using Cike.Workflow.Core.Serialization;

namespace Cike.Workflow.Application.WorkflowDefinitions;

public class WorkflowDefinitionQueryHandler(ICacheService<FolderCacheModel> folderCacheService,
    IActivitySerializer activitySerializer,
    IWorkflowDefinitionRepository workflowDefinitionRepository,
    IWorkflowValidator workflowValidator)
{
    [LocalEventHandler]
    public async Task GetListAsync(GetWorkflowDefinitionFolderListQuery query, CancellationToken cancellationToken = default)
    {
        using var _ = workflowDefinitionRepository.BeginAsNoTracking();
        Expression<Func<FolderCacheModel, bool>> filter = e => e.WorkspaceId == query.WorkspaceId && e.ParentId == query.FolderId;
        if (query.Keyword.IsNullOrEmpty() == false)
        {
            filter = e => e.WorkspaceId == query.WorkspaceId && e.ParentId == query.FolderId && e.Name.Contains(query.Keyword);
        }
        var allFolders = await folderCacheService.GetListAsync(filter, cancellationToken);

        Expression<Func<WorkflowDefinition, bool>> workflowFilter = e => e.WorkspaceId == query.WorkspaceId && e.FolderId == query.FolderId && e.IsLatest;
        if (query.Keyword.IsNullOrEmpty() == false)
        {
            workflowFilter = e => e.WorkspaceId == query.WorkspaceId && e.FolderId == query.FolderId && e.IsLatest && (e.Name.Contains(query.Keyword) || e.DefinitionId.Contains(query.Keyword));
        }
        var latestWorkflows = await workflowDefinitionRepository.GetListAsync(workflowFilter, query.Sorting, cancellationToken);

        var draftDefinitionIds = latestWorkflows.Where(x => !x.IsPublished).Select(x => x.DefinitionId).ToList();
        var publishedVersionMap = draftDefinitionIds.Count > 0
            ? await workflowDefinitionRepository.GetPublishedVersionMapAsync(draftDefinitionIds, cancellationToken)
            : new Dictionary<string, int>();

        query.Result = new List<WorkflowDefinitionFolderItemDto>();
        foreach (var item in allFolders.AsQueryable().OrderBy(query.Sorting))
        {
            var dto = item.Adapt<WorkflowDefinitionFolderItemDto>();
            dto.Type = WorkflowDefinitionFolderBaseType.Folder;
            dto.Data = new FolderItemDto
            {
                Name = item.Name,
                Path = item.BuildPath(allFolders).Select(e => new FolderPathDto { Id = e.Id, Name = e.Name }).ToList()
            };
            query.Result.Add(dto);
        }
        foreach (var item in latestWorkflows)
        {
            var data = item.Adapt<WorkflowDefinitionItemDto>();
            data.PublishedVersion = item.IsPublished ? item.Version : publishedVersionMap.GetValueOrDefault(item.DefinitionId);
            query.Result.Add(new WorkflowDefinitionFolderItemDto
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,
                CreatedBy = item.CreatedBy,
                UpdatedBy = item.UpdatedBy,
                UpdatedAt = item.UpdatedAt,
                Type = WorkflowDefinitionFolderBaseType.WorkflowDefinition,
                Data = data
            });
        }
    }

    [LocalEventHandler]
    public async Task GetAsync(GetWorkflowDefinitionQuery query, CancellationToken cancellationToken)
    {
        using var _ = workflowDefinitionRepository.BeginAsNoTracking();
        var entity = await workflowDefinitionRepository.GetAsync(query.Id, cancellationToken);
        query.Result = entity.Adapt<WorkflowDefinitionDetailDto>();
        query.Result.Root = activitySerializer.Deserialize<IActivity>(entity!.OriginalStringData);
    }

    [LocalEventHandler]
    public async Task GetVersionListAsync(GetWorkflowDefinitionVersionListQuery query, CancellationToken cancellationToken = default)
    {
        using var _ = workflowDefinitionRepository.BeginAsNoTracking();
        var versions = await workflowDefinitionRepository.GetListAsync(
            x => x.DefinitionId == query.DefinitionId, "Version desc", cancellationToken);

        query.Result = versions.Adapt<List<WorkflowDefinitionVersionItemDto>>();
    }

    [LocalEventHandler]
    public void ValidateCanvasAsync(ValidateWorkflowCanvasQuery query, CancellationToken cancellationToken = default)
    {
        var variables = query.Options.Variables
            .Select(x => new WorkflowVariableDefinition(x.Id, x.Name, x.TypeName, x.IsArray))
            .ToList();
        var errors = workflowValidator.Validate(new WorkflowValidationContext(query.Root, variables));

        query.Result = errors
            .Select(x => new WorkflowCanvasValidationErrorDto { ActivityId = x.ActivityId, Message = x.Message })
            .ToList();
    }
}
