using Cike.Workflow.Core.Models;
using Elsa.Expressions.Models;

namespace Cike.Workflow.Core.Activities;

public abstract class CodeActivity : Activity
{
    /// <inheritdoc />
    protected CodeActivity(int version = 1) : base( version)
    {
    }
}

/// <summary>
/// Base class for custom activities with auto-complete behavior that return a result.
/// </summary>
public abstract class CodeActivityWithResult : CodeActivity
{
    /// <inheritdoc />
    protected CodeActivityWithResult(string? source = default, int? line = default) : base(source, line)
    {
    }

    /// <inheritdoc />
    protected CodeActivityWithResult(string activityType, int version = 1, string? source = default, int? line = default) : base(activityType, version, source, line)
    {
    }

    /// <inheritdoc />
    protected CodeActivityWithResult(MemoryBlockReference? output, string? source = default, int? line = default) : base(source, line)
    {
        if (output != null) Result = new Output(output);
    }

    /// <inheritdoc />
    protected CodeActivityWithResult(Output? output, string? source = default, int? line = default) : base(source, line)
    {
        Result = output;
    }

    /// <summary>
    /// The result of the activity.
    /// </summary>
    public Output? Result { get; set; }
}

/// <summary>
/// Base class for custom activities with auto-complete behavior that return a result.
/// </summary>
public abstract class CodeActivity<T> : CodeActivity, IActivityWithResult<T>
{
    /// <inheritdoc />
    protected CodeActivity(string? source = default, int? line = default) : base(source, line)
    {
    }

    /// <inheritdoc />
    protected CodeActivity(string activityType, int version = 1, string? source = default, int? line = default) : base(activityType, version, source, line)
    {
    }

    /// <inheritdoc />
    protected CodeActivity(MemoryBlockReference? output, string? source = default, int? line = default) : this(source, line)
    {
        if (output != null) Result = new Output<T>(output);
    }

    /// <inheritdoc />
    protected CodeActivity(Output<T>? output, string? source = default, int? line = default) : this(source, line)
    {
        Result = output;
    }

    /// <summary>
    /// The result of the activity.
    /// </summary>
    [Output] public Output<T>? Result { get; set; }

    Output? IActivityWithResult.Result
    {
        get => Result;
        set => Result = (Output<T>?)value;
    }
}
