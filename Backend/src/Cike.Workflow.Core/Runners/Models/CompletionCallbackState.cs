namespace Cike.Workflow.Core.Runners.Models;

public class CompletionCallbackState
{
    public CompletionCallbackState()
    {
    }

    public CompletionCallbackState(long ownerInstanceId, string childNodeId, string? methodName)
    {
        OwnerInstanceId = ownerInstanceId;
        ChildNodeId = childNodeId;
        MethodName = methodName;
    }

    public long OwnerInstanceId { get; init; } = default!;

    public string ChildNodeId { get; init; } = default!;

    public string? MethodName { get; init; }
}
