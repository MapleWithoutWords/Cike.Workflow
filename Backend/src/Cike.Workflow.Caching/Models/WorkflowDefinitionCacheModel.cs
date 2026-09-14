using Cike.Contracts.EntityDtos;
using Cike.Data;
using Cike.Workflow.Domain.Shared.Enums;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Domain.Shared.CacheModels;

/// <summary>
/// 工作流定义运行时缓存模型：携带实体全部标量字段 + Options + OriginalStringData 画布全文，
/// 供 WorkflowRuntime 按 DefinitionId + Version 直接取用（拿到即可喂物化器）。
/// 与 <see cref="FolderCacheModel"/> 同目录同风格；不缓存物化后的 WorkflowActivity。
/// </summary>
public class WorkflowDefinitionCacheModel : FullAuditedEntityDto<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public long WorkspaceId { get; set; }

    public long FolderId { get; set; }

    public string DefinitionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public WorkflowDefinitionType Type { get; set; }

    public bool UsableAsActivity { get; set; }

    public string MaterializerName { get; set; } = null!;

    public string OriginalStringData { get; set; } = null!;

    public WorkflowDefinitionOptionsValueObject Options { get; set; } = new();

    public bool IsReadonly { get; set; }

    public bool IsSystem { get; set; }

    public int Version { get; set; }

    public bool IsLatest { get; set; }

    public bool IsPublished { get; set; }

    public string PublishedNote { get; set; } = string.Empty;

    public Guid PublishedBy { get; set; } = Guid.Empty;

    public DateTime PublishedAt { get; set; } = default;
}
