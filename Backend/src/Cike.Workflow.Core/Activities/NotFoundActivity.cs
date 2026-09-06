using Cike.Workflow.Core.Exceptions;
using System.Runtime.CompilerServices;

namespace Cike.Workflow.Core.Activities;

/// <summary>
/// This activity is instantiated in case a workflow references an activity type that could not be found.
/// </summary>
[Browsable(false)]
[Activity("Cike", "System", "A placeholder activity that will be used in case a workflow definition references an activity type that cannot be found.")]
public class NotFoundActivity : AutoCompleteActivity
{
    /// <inheritdoc />
    public NotFoundActivity() : base()
    {
    }

    /// <inheritdoc />
    public NotFoundActivity(string missingTypeName) : this()
    {
        MissingTypeName = missingTypeName;
    }

    /// <summary>
    /// The type name of the missing activity type.
    /// </summary>
    public string MissingTypeName { get; set; } = null!;

    /// <summary>
    /// The version of the missing activity type.
    /// </summary>
    public int MissingTypeVersion { get; set; }

    /// <summary>
    /// The original activity JSON.
    /// </summary>
    public string OriginalActivityJson { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        throw new ActivityNotFoundException(MissingTypeName, MissingTypeVersion);
    }
}
