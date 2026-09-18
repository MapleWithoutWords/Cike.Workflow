using Cike.Contracts.EntityDtos;
using Cike.Data;
using Cike.Workflow.Common.Versions;
using Cike.Workflow.Domain.Shared.Enums;

namespace Cike.Workflow.Domain.Shared.CacheModels;

/// <summary>
/// 工作流定义运行时缓存模型：携带实体全部标量字段 + OptionsPayload（IPayloadSerializer 序列化的
/// Options JSON）+ OriginalStringData 画布全文，供 WorkflowRuntime 按
/// <see cref="Cike.Workflow.Core.Models.WorkflowDefinitionHandle"/> 寻址取用；
/// Options 消费方用 IPayloadSerializer 反序列化（与 DB 影子列同一链路）。
/// 与 <see cref="FolderCacheModel"/> 同目录同风格；不缓存物化后的 WorkflowActivity。
/// 实现 <see cref="IVersion"/> 以复用 <see cref="Cike.Workflow.Common.Extensions.IVersionExtensions"/>
/// 的版本解析（与 DB 查询同源语义）。
/// </summary>
public class WorkflowDefinitionCacheModel : FullAuditedEntityDto<long>, IMultiTenant, IVersion
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

    /// <summary>
    /// Options 的 JSON 载荷——必须用 IPayloadSerializer 序列化 / 反序列化（与 DB 影子列 SerializedOptions
    /// 同源同链路）：其内部含 Expression.Value / CustomProperties 等 object 多态成员，依赖项目注册的
    /// PolymorphicObjectConverter / TypeJsonConverter 还原类型；缓存客户端自带序列化器没有这些转换器，
    /// 直接挂对象图会类型漂移，故缓存只存不透明字符串。
    /// </summary>
    public string OptionsPayload { get; set; } = "{}";

    public bool IsReadonly { get; set; }

    public bool IsSystem { get; set; }

    public int Version { get; set; }

    public bool IsLatest { get; set; }

    public bool IsPublished { get; set; }

    public string PublishedNote { get; set; } = string.Empty;

    public Guid PublishedBy { get; set; } = Guid.Empty;

    public DateTime PublishedAt { get; set; } = default;
}
