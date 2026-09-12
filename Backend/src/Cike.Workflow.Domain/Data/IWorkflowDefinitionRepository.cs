using System.Linq.Expressions;

namespace Cike.Workflow.Domain.Data;

public interface IWorkflowDefinitionRepository : IRepository<WorkflowDefinition, long>
{
    /// <summary>
    /// 带排序的列表查询（兼容迁移前的 Store 签名，排序经 System.Linq.Dynamic.Core 解析）。
    /// 注意：与迁移前一致，本方法不反序列化影子属性。
    /// </summary>
    Task<List<WorkflowDefinition>> GetListAsync(Expression<Func<WorkflowDefinition, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default);
}
