using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Attributes;
using Cike.Workflow.Core.Contexts;

namespace Cike.Workflow.Core.Tests.Helpers;

/// <summary>
/// Test activity: creates a bookmark and never completes, suspending the workflow.
/// Used to build a real suspend → resume scenario.
/// </summary>
[Activity("Test", "Helpers", "Creates a bookmark and never completes, suspending the workflow.")]
public class SuspendingActivity : Activity
{
    protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        context.CreateBookmark(new object(), callback: null);
        return ValueTask.CompletedTask;
    }
}
