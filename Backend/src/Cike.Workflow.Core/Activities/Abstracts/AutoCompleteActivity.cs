namespace Cike.Workflow.Core.Activities.Abstracts;

public abstract class AutoCompleteActivity : Activity
{
    protected AutoCompleteActivity() : base()
    {
        Behaviors.Add<AutoCompleteBehavior>(this);
    }
}

public abstract class AutoCompleteActivity<T> : AutoCompleteActivity, IActivityWithResult<T>
{
    protected AutoCompleteActivity(MemoryBlockReference? output) : base()
    {
        if (output != null) Result = new Output<T>(output);
    }

    /// <inheritdoc />
    protected AutoCompleteActivity(Output<T>? output) : base()
    {
        Result = output;
    }

    [Output]
    public Output<T>? Result { get; set; }

    Output? IActivityWithResult.Result
    {
        get => Result;
        set => Result = (Output<T>?)value;
    }
}
