namespace Cike.Workflow.Core.Activities.Abstracts;

internal record SignalHandlerRegistration(Type SignalType, Func<object, SignalContext, ValueTask> Handler);

public interface ISignalHandler
{
    /// <summary>
    /// Receives a signal.
    /// </summary>
    ValueTask ReceiveSignalAsync(object signal, SignalContext context);
}
