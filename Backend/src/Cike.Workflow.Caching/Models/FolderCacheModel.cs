using Cike.Contracts.EntityDtos;
using Cike.Data;

namespace Cike.Workflow.Domain.Shared.CacheModels;

public class FolderCacheModel : FullAuditedEntityDto<long>, IMultiTenant
{
    public long TenantId { get; set; }

    public long WorkspaceId { get; set; }

    public string Name { get; set; } = null!;

    public long ParentId { get; set; }

    public List<FolderCacheModel> BuildPath(IEnumerable<FolderCacheModel> allFolders)
    {
        List<FolderCacheModel> path = new List<FolderCacheModel>();

        var parent = allFolders.FirstOrDefault(e => e.Id == this.ParentId);
        while (parent != null)
        {
            path.Add(parent);

            parent = allFolders.FirstOrDefault(e => e.Id == parent.ParentId);
        }
        path.Reverse();
        return path;
    }
}
