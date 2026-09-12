namespace Cike.EntityFrameworkCore.Repositories;

/// <summary>
/// 带影子属性序列化的仓储基类：写路径（插入与更新）把对象图序列化进影子属性，单查路径还原。
/// 框架的单数写方法内部委托批量方法（InsertAsync → InsertManyAsync 等），覆写批量方法即可覆盖全部写路径。
/// </summary>
public abstract class SerializedEfCoreRepository<TDbContext, TEntity>(TDbContext dbContext)
    : EfCoreRepository<TDbContext, TEntity, long>(dbContext)
    where TDbContext : CikeDbContext<TDbContext>
    where TEntity : class, IEntity<long>
{
    public override async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
        }
        await base.InsertManyAsync(entities, autoSave, cancellationToken);
    }

    public override async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        foreach (var item in entities)
        {
            await OnSaveAsync(item, cancellationToken);
        }
        await base.UpdateManyAsync(entities, autoSave, cancellationToken);
    }

    public override async Task<TEntity?> FindAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await base.FindAsync(id, cancellationToken);
        await OnLoadAsync(result, cancellationToken);
        return result;
    }

    /// <summary>写路径钩子：把实体对象图序列化进影子属性。</summary>
    protected abstract ValueTask OnSaveAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>读路径钩子：把影子属性还原到实体对象图（实体不存在时传 null，实现可自行短路）。</summary>
    protected abstract ValueTask OnLoadAsync(TEntity? entity, CancellationToken cancellationToken);
}
