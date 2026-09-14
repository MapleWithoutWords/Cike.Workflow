using Cike.Workflow.Caching;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Domain.Shared.CacheModels;
using Cike.Workflow.Domain.Shared.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace Cike.EntityFrameworkCore.Repositories;

/// <summary>
/// 工作流定义仓储：影子属性序列化 + 运行时缓存 write-through（写库成功后同步条目与按定义索引）。
/// <para>
/// 两条纪律：
/// ① 经本仓储 Update 进缓存的实体，其 <c>Options</c> 必须是已还原对象图的行——取行须经
///    <c>FindAsync</c>（OnLoadAsync 还原影子属性），用 <c>GetQueryable()</c> / 列表重载取回的未还原实体
///    不得回写，否则空 Options 会被静默刷进影子列与缓存（与 MoveWorkflowDefinitionCommandHandler 的警示同源）。
/// ② 免失效兜底依据（见 .scratch/workflow-definition-runtime-cache/PRD.md）：已发布版本行不可变
///    （改动走新版本），WorkflowRuntime 只读已发布条目、草稿调试实例在 Application 层建好后交调度，
///    故不依赖 TTL / DB 回退 / 跨实例失效广播。
/// 已知取舍（与 Folder / Workspace 的 <c>CachedEfCoreRepository</c> 同源）：缓存同步发生在
/// SaveChanges 之后、请求管道最终提交之前——极端情况下（提交失败回滚）可能残留幻影条目；
/// 对"发布是显式操作 + 已发布行不可变"的场景风险可控。
/// </para>
/// </summary>
public class WorkflowDefinitionRepository(CikeWorkflowDbContenxt context, IPayloadSerializer payloadSerializer,
    ILogger<WorkflowDefinitionRepository> logger, IWorkflowDefinitionCache workflowDefinitionCache)
    : SerializedEfCoreRepository<CikeWorkflowDbContenxt, WorkflowDefinition>(context), IWorkflowDefinitionRepository, IScopedDependency
{
    public override async Task InsertManyAsync(IEnumerable<WorkflowDefinition> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await base.InsertManyAsync(entities, autoSave, cancellationToken);
        await SetCacheAsync(entities, cancellationToken);
    }

    public override async Task UpdateManyAsync(IEnumerable<WorkflowDefinition> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await base.UpdateManyAsync(entities, autoSave, cancellationToken);
        await SetCacheAsync(entities, cancellationToken);
    }

    public override async Task DeleteManyAsync(IEnumerable<WorkflowDefinition> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await base.DeleteManyAsync(entities, autoSave, cancellationToken);
        foreach (var entity in entities)
        {
            await workflowDefinitionCache.RemoveAsync(entity.DefinitionId, entity.Version, cancellationToken);
        }
    }

    /// <summary>框架的单数写方法内部委托批量方法，覆写批量口即覆盖全部写路径（与 CachedEfCoreRepository 同理）。</summary>
    private async Task SetCacheAsync(IEnumerable<WorkflowDefinition> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            var model = entity.Adapt<WorkflowDefinitionCacheModel>();
            // Options 走与影子列同一个 IPayloadSerializer：保留 Expression.Value / CustomProperties
            // 等 object 多态成员的类型保真；缓存客户端自带序列化器没有项目的转换器，只准存不透明字符串。
            model.OptionsPayload = payloadSerializer.Serialize(entity.Options);
            await workflowDefinitionCache.SetAsync(model, cancellationToken);
        }
    }

    /// <summary>与迁移前一致：列表查询不反序列化影子属性，仅排序。</summary>
    public async Task<List<WorkflowDefinition>> GetListAsync(Expression<Func<WorkflowDefinition, bool>> filter, string sorting = "CreatedAt desc", CancellationToken cancellationToken = default)
    {
        return await GetQueryable().AsNoTracking().Where(filter).OrderBy(sorting).ToListAsync(cancellationToken);
    }

    protected override ValueTask OnSaveAsync(WorkflowDefinition entity, CancellationToken cancellationToken)
    {
        DbContext.Entry(entity).Property("SerializedOptions").CurrentValue = payloadSerializer.Serialize(entity.Options);
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnLoadAsync(WorkflowDefinition? entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            return ValueTask.CompletedTask;

        var json = (string?)DbContext.Entry(entity).Property("SerializedOptions").CurrentValue;

        try
        {
            if (!string.IsNullOrWhiteSpace(json))
                entity.Options = payloadSerializer.Deserialize<WorkflowDefinitionOptionsValueObject>(json);
        }
        catch (Exception exp)
        {
            logger.LogError(exp, "Could not deserialize workflow definition state: {DefinitionId}. Reverting to default state", entity.DefinitionId);
        }

        return ValueTask.CompletedTask;
    }
}
