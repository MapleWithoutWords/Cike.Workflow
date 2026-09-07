using Cike.Data;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Variables;
using Cike.Workflow.Domain.Data.ValueObjects;
using Cike.Workflow.Domain.Shared.Enums;

namespace Cike.Workflow.Domain.Data.Entities;

public class WorkflowDefinition : FullAuditedAggregateRoot<long>, IMultiTenant
{
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

    public long FolderId { get; set; }

    public long TenantId { get; set; }
}
