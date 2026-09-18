using Cike.EntityFrameworkCore.Repositories;
using Cike.Workflow.Core.Helpers;
using Cike.Workflow.Core.Serialization;
using Quartz;
using System.Linq.Dynamic.Core;

namespace Cike.Workflow.EntityFrameworkCore.Repositories
{
    internal class TriggerRepository(CikeWorkflowDbContext context, IStimulusHasher stimulusHasher, IPayloadSerializer payloadSerializer) : SerializedEfCoreRepository<CikeWorkflowDbContext, TriggerEntity>(context), ITriggerRepository, IScopedDependency
    {
        public Task<IEnumerable<TriggerEntity>> FindTriggersAsync(string activityTypeName, object stimulus, CancellationToken cancellationToken = default)
        {
            var hash = stimulusHasher.Hash(activityTypeName, stimulus);
            return FindTriggersAsync(hash, cancellationToken);
        }

        public async Task<IEnumerable<TriggerEntity>> FindTriggersAsync(string stimulusHash, CancellationToken cancellationToken = default)
        {
            return await ToListAsync(GetQueryable().Where(e => e.Hash == stimulusHash), cancellationToken);
        }

        public override async Task<List<TriggerEntity>> ToListAsync(IQueryable<TriggerEntity> query, CancellationToken cancellationToken = default)
        {
            var result = await base.ToListAsync(query, cancellationToken);
            foreach (var item in result)
            {
                await OnLoadAsync(item, cancellationToken);
            }
            return result;
        }

        public override async Task<(long Total, List<TriggerEntity> Items)> ToPagedListAsync(IQueryable<TriggerEntity> query, IPagedAndSortedRequest request, CancellationToken cancellationToken = default)
        {
            var pageResult = await base.ToPagedListAsync(query, request, cancellationToken);
            foreach (var item in pageResult.Items)
            {
                await OnLoadAsync(item, cancellationToken);
            }
            return pageResult;
        }

        protected override ValueTask OnSaveAsync(TriggerEntity entity, CancellationToken cancellationToken)
        {
            DbContext.Entry(entity).Property("SerializedPayload").CurrentValue = entity.Payload != null ? payloadSerializer.Serialize(entity.Payload) : null;
            return default;
        }

        protected override ValueTask OnLoadAsync(TriggerEntity? entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                return default;

            var payloadJson = DbContext.Entry(entity).Property<string>("SerializedPayload").CurrentValue;
            entity.Payload = !string.IsNullOrEmpty(payloadJson) ? payloadSerializer.Deserialize(payloadJson) : null;

            return default;
        }
    }
}
