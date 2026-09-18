namespace Cike.Workflow.Domain.Data.Entities;

public class TriggerEntity : AuditedEntity<long>, IMultiTenant
{
    public string WorkflowDefinitionId { get; set; } = null!;

    public long WorkflowDefinitionVersionId { get; set; }

    public string Name { get; set; } = null!;

    public string ActivityId { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public object? Payload { get; set; } = null!;

    public long TenantId { get; set; }
}
