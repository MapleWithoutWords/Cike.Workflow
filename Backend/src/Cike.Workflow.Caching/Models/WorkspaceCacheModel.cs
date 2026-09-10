namespace Cike.Workflow.Caching.Models;

public class WorkspaceCacheModel : FullAuditedEntityDto<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
}
