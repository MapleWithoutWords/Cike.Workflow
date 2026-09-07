namespace Cike.Workflow.Domain.Data.Entities;

public class Folder : FullAuditedAggregateRoot<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public string Name { get; set; } = null!;

    public long ParentId { get; set; }
}
