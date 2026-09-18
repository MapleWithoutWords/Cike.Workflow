using Cike.Core.DependencyInjection;
using Cike.Workflow.Caching;
using Cike.Workflow.Common.Extensions;
using Cike.Workflow.Common.Versions;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.Serialization;
using Cike.Workflow.Core.WorkflowGraphs;
using Cike.Workflow.Core.WorkflowGraphs.Models;
using Cike.Workflow.Domain.Data;
using Cike.Workflow.Domain.Data.Entities;
using Cike.Workflow.Domain.Materializers;
using Cike.Workflow.Domain.Shared.CacheModels;
using Cike.Workflow.Domain.Shared.ValueObjects;

namespace Cike.Workflow.Runtime.WorkflowDefintions
{
    /// <summary>
    /// 定义运行时读链路：定义行优先取 <see cref="IWorkflowDefinitionCache"/>，未命中回落数据库
    /// （行 Id 经 GetListAsync 定位后必须走 <c>FindAsync</c> 取回——只有它会经 OnLoadAsync 还原
    /// 影子属性 Options；缓存回落数据库不回填，写口只归仓储 write-through）。
    /// WorkflowGraph 物化结果进 <see cref="WorkflowGraphCache"/>（滑动 30 分钟）。
    /// </summary>
    internal class WorkflowDefinitionService(
        IWorkflowDefinitionCache workflowDefinitionCache,
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IMaterializerManager materializerManager,
        IWorkflowGraphBuilder workflowGraphBuilder,
        IPayloadSerializer payloadSerializer,
        WorkflowGraphCache workflowGraphCache) : IWorkflowDefinitionService, IScopedDependency
    {
        public async Task<WorkflowDefinition?> FindWorkflowDefinitionAsync(string definitionId, VersionOptions versionOptions, CancellationToken cancellationToken = default)
        {
            var cached = await workflowDefinitionCache.GetAsync(WorkflowDefinitionHandle.ByDefinitionId(definitionId, versionOptions), cancellationToken);
            if (cached != null)
                return ToDefinition(cached);

            // 缓存未命中：版本过滤下推数据库，单行寻址（FindWorkflowDefinitionAsync 内部经 FindAsync 还原影子属性 Options）
            return await workflowDefinitionRepository.FindWorkflowDefinitionAsync(definitionId, versionOptions, cancellationToken);
        }

        public async Task<WorkflowDefinition?> FindWorkflowDefinitionAsync(long definitionVersionId, CancellationToken cancellationToken = default)
        {
            var cached = await workflowDefinitionCache.GetAsync(WorkflowDefinitionHandle.ByDefinitionVersionId(definitionVersionId), cancellationToken);
            if (cached != null)
                return ToDefinition(cached);

            return await workflowDefinitionRepository.FindAsync(definitionVersionId, cancellationToken);
        }

        public async Task<WorkflowDefinition?> FindWorkflowDefinitionAsync(WorkflowDefinitionHandle handle, CancellationToken cancellationToken = default)
        {
            if (handle.DefinitionVersionId != null)
                return await FindWorkflowDefinitionAsync(handle.DefinitionVersionId.Value, cancellationToken);

            if (handle.DefinitionId != null)
                return await FindWorkflowDefinitionAsync(handle.DefinitionId, handle.VersionOptions ?? default, cancellationToken);

            throw new ArgumentException($"Invalid WorkflowDefinitionHandle: {handle}", nameof(handle));
        }

        public async Task<WorkflowGraph?> FindWorkflowGraphAsync(long definitionVersionId, CancellationToken cancellationToken = default)
        {
            var definition = await FindWorkflowDefinitionAsync(definitionVersionId, cancellationToken);
            return await TryMaterializeWorkflowAsync(definition, cancellationToken);
        }

        public async Task<WorkflowGraph> GetWorkflowGraphAsync(WorkflowDefinitionHandle definitionHandle, CancellationToken cancellationToken = default)
        {
            var definition = await FindWorkflowDefinitionAsync(definitionHandle, cancellationToken)
                ?? throw new InvalidOperationException($"Workflow definition not found: {definitionHandle}");

            return await MaterializeWorkflowAsync(definition, cancellationToken);
        }

        private async Task<WorkflowGraph?> TryMaterializeWorkflowAsync(WorkflowDefinition? definition, CancellationToken cancellationToken)
        {
            if (definition == null)
                return null;

            if (materializerManager.IsMaterializerAvailable(definition.MaterializerName))
                return await MaterializeWorkflowAsync(definition, cancellationToken);

            return null;
        }

        public async Task<WorkflowGraph> MaterializeWorkflowAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
        {
            return await workflowGraphCache.GetOrCreateAsync(definition.Id, async () =>
            {
                var materializer = materializerManager.GetMaterializer(definition.MaterializerName)
                    ?? throw new InvalidOperationException($"Workflow materializer not found: {definition.MaterializerName}");

                var workflow = await materializer.MaterializeAsync(definition, cancellationToken);
                return await workflowGraphBuilder.BuildAsync(workflow, cancellationToken);
            });
        }

        /// <summary>缓存模型 → 实体：Options 载荷经同一 IPayloadSerializer 还原（与 DB 影子列同链路）。</summary>
        private WorkflowDefinition ToDefinition(WorkflowDefinitionCacheModel model)
        {
            return new WorkflowDefinition
            {
                Id = model.Id,
                TenantId = model.TenantId,
                WorkspaceId = model.WorkspaceId,
                FolderId = model.FolderId,
                DefinitionId = model.DefinitionId,
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                UsableAsActivity = model.UsableAsActivity,
                MaterializerName = model.MaterializerName,
                OriginalStringData = model.OriginalStringData,
                IsReadonly = model.IsReadonly,
                IsSystem = model.IsSystem,
                Version = model.Version,
                IsLatest = model.IsLatest,
                IsPublished = model.IsPublished,
                PublishedNote = model.PublishedNote,
                PublishedBy = model.PublishedBy,
                PublishedAt = model.PublishedAt,
                Options = payloadSerializer.Deserialize<WorkflowDefinitionOptionsValueObject>(model.OptionsPayload)
            };
        }
    }
}
