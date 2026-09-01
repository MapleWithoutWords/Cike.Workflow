using Cike.Workflow.Core.Activities.Behaviors;
using Cike.Workflow.Core.Extensions;
using Cike.Workflow.Core.Helpers;
using System.Text.Json.Serialization;

namespace Cike.Workflow.Core.Activities.Abstracts;

public abstract class Activity : IActivity, ISignalHandler
{
    private readonly ICollection<SignalHandlerRegistration> _signalReceivedHandlers = new List<SignalHandlerRegistration>();

    public Activity()
    {
        Type = ActivityTypeNameHelper.GenerateTypeName(GetType());
        Version = 1;
        Behaviors.Add<ScheduledChildCallbackBehavior>(this);
    }

    public string Id { get; set; } = null!;

    public string NodeId { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Name { get; set; }

    public string Type { get; set; } = null!;

    public int Version { get; set; }

    public IDictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    [JsonIgnore]
    public ICollection<IBehavior> Behaviors { get; } = new List<IBehavior>();

    async ValueTask<bool> IActivity.CanExecuteAsync(ActivityExecutionContext context)
    {
        return await CanExecuteAsync(context);
    }

    protected virtual ValueTask<bool> CanExecuteAsync(ActivityExecutionContext context)
    {
        return ValueTask.FromResult(true);
    }

    async ValueTask IActivity.ExecuteAsync(ActivityExecutionContext context)
    {
        await ExecuteAsync(context);

        // Invoke behaviors.
        foreach (var behavior in Behaviors) await behavior.ExecuteAsync(context);
    }

    protected virtual ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        return ValueTask.CompletedTask;
    }

    async ValueTask ISignalHandler.ReceiveSignalAsync(object signal, SignalContext context)
    {
        // Give derived activity a chance to do something with the signal.
        await OnSignalReceivedAsync(signal, context);

        // Invoke registered signal delegates for this particular type of signal.
        var signalType = signal.GetType();
        var handlers = _signalReceivedHandlers.Where(x => x.SignalType == signalType);

        foreach (var registration in handlers)
            await registration.Handler(signal, context);

        // Invoke behaviors.
        foreach (var behavior in Behaviors) await behavior.ReceiveSignalAsync(signal, context);
    }

    protected virtual ValueTask OnSignalReceivedAsync(object signal, SignalContext context)
    {
        return ValueTask.CompletedTask;
    }

    #region Subscribe Signal
    protected void OnSignalReceived(Type signalType, Func<object, SignalContext, ValueTask> handler) => _signalReceivedHandlers.Add(new SignalHandlerRegistration(signalType, handler));

    protected void OnSignalReceived<T>(Func<T, SignalContext, ValueTask> handler) => OnSignalReceived(typeof(T), (signal, context) => handler((T)signal, context));

    protected void OnSignalReceived<T>(Action<T, SignalContext> handler)
    {
        OnSignalReceived<T>((signal, context) =>
        {
            handler(signal, context);
            return ValueTask.CompletedTask;
        });
    }
    #endregion
}

public abstract class Activity<T> : Activity, IActivityWithResult<T>
{
    /// <inheritdoc />
    protected Activity() : base()
    {
    }

    /// <inheritdoc />
    protected Activity(MemoryBlockReference? output) : this()
    {
        if (output != null) Result = new Output<T>(output);
    }

    /// <inheritdoc />
    protected Activity(Output<T>? output) : this()
    {
        Result = output;
    }

    /// <summary>
    /// The result of the activity.
    /// </summary>
    public Output<T>? Result { get; set; }

    Output? IActivityWithResult.Result
    {
        get => Result;
        set => Result = (Output<T>?)value;
    }
}
