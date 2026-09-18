namespace Cike.Workflow.Domain.Data;

public interface IFolderRepository : IRepository<Folder, long>
{
    /// <summary>取某工作空间全部目录的父子映射（数据库端投影 Id/ParentId），供移动防环上溯。</summary>
    Task<Dictionary<long, long>> GetParentMapAsync(long workspaceId, CancellationToken cancellationToken = default);
}
