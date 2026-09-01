namespace Cike.Workflow.Core.Runners.Models;

public record ExceptionState(
    string TypeName,
    string Message,
    string? StackTrace,
    ExceptionState? InnerException)
{
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    [JsonConstructor]
    public ExceptionState() : this(default!, default!, default, default)
    {

    }

    public static ExceptionState? FromException(Exception? ex)
    {
        return ex == null ? null : new ExceptionState(ex.GetType().FullName!, ex.Message, ex.StackTrace, FromException(ex.InnerException));
    }
}
