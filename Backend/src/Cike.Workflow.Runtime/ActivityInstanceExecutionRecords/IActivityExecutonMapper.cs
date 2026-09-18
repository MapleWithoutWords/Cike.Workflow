using Cike.Workflow.Core.Contexts;

namespace Cike.Workflow.Runtime.ActivityInstanceExecutionRecords;

public interface IActivityExecutionMapper
{
    ActivityInstanceExecutionRecord Map(ActivityExecutionContext source);
}
