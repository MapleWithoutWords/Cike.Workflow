using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Core.Validation;

namespace Cike.Workflow.Application.WorkflowDefinitions;

public class WorkflowDefinitionCommandHandler(
    IWorkflowDefinitionRepository workflowDefinitionRepository,
    IWorkspaceRepository workspaceRepository,
    IFolderRepository folderRepository,
    IDistributedCacheClient distributedCacheClient,
    IActivitySerializer activitySerializer,
    ICurrentUser currentUser,
    IWorkflowValidator workflowValidator)
{
    private const string DefinitionIdSeqKey = "cike:workflow:workflow-definition:code:seq";

    [LocalEventHandler]
    public async Task AddAsync(AddWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var dto = command.Dto;

        if (!await workspaceRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.Id == dto.WorkspaceId, cancellationToken))
            throw new UserFriendlyException("所属工作空间不存在，请检查后重试。");

        if (dto.FolderId != 0
            && !await folderRepository.GetQueryable().AsNoTracking()
            .AnyAsync(x => x.Id == dto.FolderId && x.WorkspaceId == dto.WorkspaceId, cancellationToken))
            throw new UserFriendlyException("目录不属于该工作空间，请检查后重试。");

        if (dto.DefinitionId.IsNullOrEmpty())
        {
            dto.DefinitionId = $"WF_{await distributedCacheClient.HashIncrementAsync(DefinitionIdSeqKey, 1)}";
        }

        await ValidateDuplicateAsync(dto.WorkspaceId, dto.Name, dto.DefinitionId, null, cancellationToken);

        var entity = dto.Adapt<WorkflowDefinition>();
        entity.OriginalStringData = activitySerializer.Serialize(new Flowchart());

        await workflowDefinitionRepository.InsertAsync(entity, cancellationToken: cancellationToken);
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
        await workflowDefinitionRepository.UpdateAsync(entity, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task SaveAsync(SaveWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (entity.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许保存。");
        if (entity.IsReadonly)
            throw new UserFriendlyException("只读工作流不允许保存。");

        var latest = await GetLatestAsync(entity.DefinitionId, cancellationToken);

        var data = activitySerializer.Serialize(command.Dto.Root);

        if (!latest.IsPublished)
        {
            // 草稿就地更新：版本号、IsLatest、IsPublished 不变
            latest.OriginalStringData = data;
            latest.Options = command.Dto.Options;
            await workflowDefinitionRepository.UpdateAsync(latest, cancellationToken: cancellationToken);
            command.DraftId = latest.Id;
            return;
        }

        // 最新版已发布：生成 v+1 新草稿，IsLatest 转移，元数据沿用最新行
        latest.IsLatest = false;
        await workflowDefinitionRepository.UpdateAsync(latest, cancellationToken: cancellationToken);

        var draft = CreateDraft(latest, data, command.Dto.Options);
        await workflowDefinitionRepository.InsertAsync(draft, cancellationToken: cancellationToken);
        command.DraftId = draft.Id;
    }

    [LocalEventHandler]
    public async Task DeleteAsync(DeleteWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (entity.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许删除。");

        var versions = await workflowDefinitionRepository.GetQueryable()
            .Where(x => x.DefinitionId == entity.DefinitionId)
            .ToListAsync(cancellationToken);

        await workflowDefinitionRepository.DeleteManyAsync(versions, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task PublishAsync(PublishWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (entity.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许发布。");

        var latest = await GetLatestAsync(entity.DefinitionId, cancellationToken);

        if (latest.IsPublished)
            throw new UserFriendlyException("当前版本已发布，请先保存产生新草稿后再发布。");

        // 严格画布校验：全部通过才允许落库（Core 层 WorkflowValidator）
        var root = activitySerializer.Deserialize(latest.OriginalStringData);
        var variables = latest.Options.Variables
            .Select(x => new WorkflowVariableDefinition(x.Id, x.Name, x.TypeName, x.IsArray))
            .ToList();
        var errors = workflowValidator.Validate(new WorkflowValidationContext(root, variables));
        if (errors.Count > 0)
            throw new UserFriendlyException(string.Join("；", errors.Select(x => x.Message)));

        latest.IsPublished = true;
        latest.PublishedNote = command.PublishedNote ?? string.Empty;
        latest.PublishedBy = currentUser.GetGuidId();
        latest.PublishedAt = DateTime.Now;
        await workflowDefinitionRepository.UpdateAsync(latest, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task MoveAsync(MoveWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(command.Id, cancellationToken);

        if (entity.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许移动。");

        if (command.FolderId != 0)
        {
            var folderExists = await folderRepository.GetQueryable().AsNoTracking()
                .AnyAsync(x => x.Id == command.FolderId && x.WorkspaceId == entity.WorkspaceId, cancellationToken);
            if (!folderExists)
                throw new UserFriendlyException("目标目录不存在或不属于该工作空间，请检查后重试。");
        }

        // 该定义的所有版本行一起移动（移动不改内容，IsReadonly 不拦截）。
        // 经 FindAsync 逐行取回：UpdateManyAsync 的 OnSaveAsync 会按实体 Options 重写影子属性，
        // 必须先经 OnLoadAsync 还原，否则原始 Options 被默认空值覆盖。
        var versionIds = await workflowDefinitionRepository.GetQueryable().AsNoTracking()
            .Where(x => x.DefinitionId == entity.DefinitionId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var versions = new List<WorkflowDefinition>();
        foreach (var versionId in versionIds)
        {
            var version = await workflowDefinitionRepository.FindAsync(versionId, cancellationToken);
            if (version != null)
            {
                version.FolderId = command.FolderId;
                versions.Add(version);
            }
        }

        await workflowDefinitionRepository.UpdateManyAsync(versions, cancellationToken: cancellationToken);
    }

    [LocalEventHandler]
    public async Task RollbackAsync(RollbackWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        // 目标版本行直接经 FindAsync 取回（同时还原 Options 影子属性，回滚要复制画布内容含 Options）
        var target = await workflowDefinitionRepository.FindAsync(command.DefinitionVersionId, cancellationToken)
            ?? throw new UserFriendlyException("回滚目标版本不存在，请检查后重试。");

        if (target.DefinitionId != command.DefinitionId)
            throw new UserFriendlyException("回滚目标版本不存在，请检查后重试。");

        if (target.IsSystem)
            throw new UserFriendlyException("系统内置工作流不允许回滚。");
        if (target.IsReadonly)
            throw new UserFriendlyException("只读工作流不允许回滚。");

        var latest = await GetLatestAsync(target.DefinitionId, cancellationToken);

        if (latest.Id == target.Id)
            throw new UserFriendlyException("回滚目标版本与当前版本相同，无需回滚。");

        if (!latest.IsPublished)
        {
            // 有未发布草稿：用目标版本的画布内容覆盖草稿，版本号不变（原草稿内容丢弃——已接受的取舍）
            latest.OriginalStringData = target.OriginalStringData;
            latest.Options = target.Options;
            await workflowDefinitionRepository.UpdateAsync(latest, cancellationToken: cancellationToken);
            return;
        }

        // 最新版已发布：生成 v+1 新草稿，画布内容复制自目标版本，元数据沿用最新行
        latest.IsLatest = false;
        await workflowDefinitionRepository.UpdateAsync(latest, cancellationToken: cancellationToken);

        var draft = CreateDraft(latest, target.OriginalStringData, target.Options);
        await workflowDefinitionRepository.InsertAsync(draft, cancellationToken: cancellationToken);
    }

    private async Task ValidateDuplicateAsync(long workspaceId, string name, string? definitionId, string? excludeDefinitionId, CancellationToken cancellationToken = default)
    {
        var duplicates = await workflowDefinitionRepository.GetQueryable().AsNoTracking()
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
        return await workflowDefinitionRepository.FindAsync(id, cancellationToken)
            ?? throw new UserFriendlyException("工作流定义不存在，请检查后重试。");
    }

    /// <summary>基于最新行创建 v+1 新草稿：元数据沿用最新行，画布内容取传入值。</summary>
    private static WorkflowDefinition CreateDraft(WorkflowDefinition latest, string originalStringData, WorkflowDefinitionOptionsValueObject options)
        => new()
        {
            WorkspaceId = latest.WorkspaceId,
            FolderId = latest.FolderId,
            DefinitionId = latest.DefinitionId,
            Name = latest.Name,
            Description = latest.Description,
            Type = latest.Type,
            UsableAsActivity = latest.UsableAsActivity,
            MaterializerName = latest.MaterializerName,
            IsReadonly = latest.IsReadonly,
            IsSystem = latest.IsSystem,
            Version = latest.Version + 1,
            IsLatest = true,
            IsPublished = false,
            OriginalStringData = originalStringData,
            Options = options,
        };

    private async Task<WorkflowDefinition> GetLatestAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        // 经 FindAsync 取回，确保影子属性（SerializedOptions）反序列化还原
        var latestId = await workflowDefinitionRepository.GetQueryable().AsNoTracking()
            .Where(x => x.DefinitionId == definitionId && x.IsLatest)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return latestId == null
            ? throw new UserFriendlyException("工作流定义不存在，请检查后重试。")
            : await workflowDefinitionRepository.FindAsync(latestId.Value, cancellationToken)
              ?? throw new UserFriendlyException("工作流定义不存在，请检查后重试。");
    }
}
