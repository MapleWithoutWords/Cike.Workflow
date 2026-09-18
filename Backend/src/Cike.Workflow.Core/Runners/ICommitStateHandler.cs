using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Runners
{
    public interface ICommitStateHandler
    {
        Task CommitAsync(WorkflowExecutionContext workflowExecutionContext, CancellationToken cancellationToken = default);
        Task CommitAsync(WorkflowExecutionContext workflowExecutionContext, WorkflowState workflowState, CancellationToken cancellationToken = default);
    }
}
