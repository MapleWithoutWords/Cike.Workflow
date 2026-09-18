using Cike.Core.DependencyInjection;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Variables;
using Cike.Workflow.Domain.Managers;
using Cike.Workflow.Runtime.ActivityInstanceExecutionRecords;
using Cike.Workflow.Runtime.Bookmarks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Services;

[Dependency(ReplaceServices = true)]
internal class CommitStateHandler(
    IWorkflowStateExtractor workflowStateExtractor,
    IVariablePersistenceManager variablePersistenceManager,
    BookmarkManager bookmarkManager,
    IActivityExecutionMapper activityExecutionMapper,
    IActivityInstanceExecutionRecordRepository activityInstanceExecutionRecordRepository,
    IUnitOfWork unitOfWork,
    WorkflowInstanceManager workflowInstanceManager) : ICommitStateHandler, IScopedDependency
{
    public async Task CommitAsync(WorkflowExecutionContext workflowExecutionContext, CancellationToken cancellationToken = default)
    {
        var workflowState = workflowStateExtractor.Extract(workflowExecutionContext);
        await CommitAsync(workflowExecutionContext, workflowState, cancellationToken);
    }

    public async Task CommitAsync(WorkflowExecutionContext workflowExecutionContext, WorkflowState workflowState, CancellationToken cancellationToken = default)
    {
        WorkflowInstance? workflowInstance = null;
        var updateBookmarksRequest = new UpdateBookmarksRequest(workflowExecutionContext, workflowExecutionContext.BookmarksDiff, workflowExecutionContext.CorrelationId);
        await bookmarkManager.UpdateBookmarksAsync(updateBookmarksRequest);

        await PersistExecutionLogsAsync(workflowExecutionContext, cancellationToken);
        await variablePersistenceManager.SaveVariablesAsync(workflowExecutionContext);
        workflowInstance = await workflowInstanceManager.SaveAsync(workflowState, cancellationToken);

        ClearActivityExecutionContextTaint(workflowExecutionContext);
        workflowExecutionContext.ClearCompletedActivityExecutionContexts();

        await unitOfWork.CommitAsync(cancellationToken);
    }

    private static void ClearActivityExecutionContextTaint(WorkflowExecutionContext workflowExecutionContext)
    {
        foreach (var activityExecutionContext in workflowExecutionContext.ActivityExecutionContexts.Where(x => x.IsDirty))
            activityExecutionContext.ClearTaint();
    }

    /// <inheritdoc />
    public async Task PersistExecutionLogsAsync(WorkflowExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Select tainted activity execution contexts to avoid saving untainted ones multiple times.
        var activityExecutionContexts = context.ActivityExecutionContexts.Where(x => x.IsDirty).ToList();

        if (activityExecutionContexts.Count == 0)
            return;

        var records = activityExecutionContexts.Select(x => activityExecutionMapper.Map(x));
        await activityInstanceExecutionRecordRepository.SaveManyAsync(records, cancellationToken);
    }
}
