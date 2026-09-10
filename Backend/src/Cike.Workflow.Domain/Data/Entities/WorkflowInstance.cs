using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;

namespace Cike.Workflow.Domain.Data.Entities;

public class WorkflowInstance : FullAuditedAggregateRoot<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public long WorkspaceId { get; set; }

    public string DefinitionId { get; set; } = null!;

    public long DefinitionVersionId { get; set; }

    public int Version { get; set; }

    public long ParentWorkflowInstanceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public bool IsExecuting { get; set; }

    public int IncidentCount { get; set; }

    public WorkflowStatus Status { get; set; }

    public DateTime FinishedAt { get; set; }

    public WorkflowState WorkflowState { get; set; } = null!;
}
