using Cike.Workflow.Core.Schedulers.Models;
using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.Schedulers
{
    public interface IWorkflowExecutionContextSchedulerStrategy
    {
        ActivityWorkItem Schedule(WorkflowExecutionContext context,
            ActivityNode activityNode,
            ActivityExecutionContext owner,
            ScheduleWorkOptions? options = null);
    }
}
