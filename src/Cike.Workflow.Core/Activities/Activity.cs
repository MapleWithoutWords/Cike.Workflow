namespace Cike.Workflow.Core.Activities;

public abstract class Activity : IActivity
{
    protected Activity()
    {
        Version = 1;
        //Behaviors.Add<ScheduledChildCallbackBehavior>(this);
    }

    protected Activity(int version): this()
    {
        Version = version;
    }

    public long Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public int Version { get; set; }

    /// <inheritdoc />
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    protected virtual ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(context);

        // Invoke behaviors.
        //foreach (var behavior in Behaviors) await behavior.ExecuteAsync(context);
    }
}
