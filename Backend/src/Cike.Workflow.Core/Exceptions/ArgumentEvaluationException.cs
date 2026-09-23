namespace Cike.Workflow.Core.Exceptions;

public class ArgumentEvaluationException(string argumentName, string message, Exception exception) : Exception(message, exception)
{
    public string ArgumentName { get; } = argumentName;
}
