using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities;

/// <summary>
/// Break out of a loop.
/// </summary>
[Activity("Cike", "Looping", "Break out of a loop.")]
public class Break : AutoCompleteActivity, ITerminalNode
{
    /// <inheritdoc />
    public Break() : base()
    {
    }

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Send a signal to the parent scope to break out of the loop.
        await context.SendSignalAsync(new BreakSignal());
    }
}
