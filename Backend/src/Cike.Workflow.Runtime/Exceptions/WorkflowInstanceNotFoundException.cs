namespace Cike.Workflow.Runtime.Exceptions;

public class WorkflowInstanceNotFoundException(string message, long instanceId) : Exception(message)
{
    public long InstanceId { get; } = instanceId;
}
