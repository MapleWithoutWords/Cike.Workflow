namespace Cike.Workflow.Domain.Data;

public interface IActivityInstanceExecutionRecordRepository : IRepository<ActivityInstanceExecutionRecord, long>
{
    ValueTask SaveManyAsync(IEnumerable<ActivityInstanceExecutionRecord> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按工作流实例 ID 查询活动执行记录（CreatedAt、Id 升序），影子列（活动状态/异常等）随读路径还原。
    /// </summary>
    Task<List<ActivityInstanceExecutionRecord>> FindByWorkflowInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default);
}
