using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Attributes;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Expressions.Models;

namespace Cike.Workflow.Core.Tests.Helpers;

/// <summary>
/// Test activity: writes a fixed value (possibly null) to a workflow output by name
/// (matched via the output's memory block reference id equal to the workflow output definition name).
/// Used to verify output-default semantics such as "an explicitly written null counts as written".
/// </summary>
[Activity("Test", "Helpers", "Writes a fixed value (possibly null) to a workflow output by name.")]
public class WriteWorkflowOutput(string outputName, object? value) : AutoCompleteActivity
{
    public string OutputName { get; set; } = outputName;

    public object? ValueToWrite { get; set; } = value;

    protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        context.Set(new Output(new MemoryBlockReference(OutputName)), ValueToWrite);
        return ValueTask.CompletedTask;
    }
}
