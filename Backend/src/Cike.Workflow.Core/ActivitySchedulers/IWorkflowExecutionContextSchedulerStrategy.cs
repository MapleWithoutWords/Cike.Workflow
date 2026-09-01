using Cike.Workflow.Core.ActivitySchedulers.Models;

namespace Cike.Workflow.Core.ActivitySchedulers
{
    public interface IWorkflowExecutionContextSchedulerStrategy
    {
        ActivityWorkItem Schedule(WorkflowExecutionContext context,
            ActivityNode activityNode,
            ActivityExecutionContext owner,
            ScheduleWorkOptions? options = null);
    }
}
