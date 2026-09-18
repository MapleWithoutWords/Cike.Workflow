using Cike.Uow;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Domain.Data;
using Cike.Workflow.Domain.Managers.Mappers;

namespace Cike.Workflow.Domain.Managers
{
    public class WorkflowInstanceManager(
    IWorkflowInstanceRepository store,
    WorkflowInstanceFactory workflowInstanceFactory,
    IUnitOfWork unitOfWork,
    WorkflowStateMapper workflowStateMapper) : IScopedDependency
    {
        public async Task<WorkflowInstance?> FindByIdAsync(long instanceId, CancellationToken cancellationToken = default)
        {
            return await store.FindAsync(instanceId, cancellationToken);
        }

        public async Task<WorkflowInstance> SaveAsync(WorkflowState workflowState, CancellationToken cancellationToken)
        {
            var workflowInstance = workflowStateMapper.Map(workflowState)!;
            var instanceExists = await store.AnyAsync(e => e.Id == workflowState.Id, cancellationToken);
            if (instanceExists)
            {
                await store.UpdateAsync(workflowInstance, cancellationToken: cancellationToken);
            }
            else
            {
                await store.InsertAsync(workflowInstance, cancellationToken: cancellationToken);
            }
            return workflowInstance;
        }

        public async Task<WorkflowInstance> CreateAndCommitWorkflowInstanceAsync(WorkflowActivity workflow, WorkflowInstanceOptions? options = null, CancellationToken cancellationToken = default)
        {
            var workflowInstance = CreateWorkflowInstance(workflow, options);
            await store.InsertAsync(workflowInstance, cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return workflowInstance;
        }

        /// <inheritdoc />
        private WorkflowInstance CreateWorkflowInstance(WorkflowActivity workflow, WorkflowInstanceOptions? options = null)
        {
            return workflowInstanceFactory.CreateWorkflowInstance(workflow, options);
        }
    }
}
