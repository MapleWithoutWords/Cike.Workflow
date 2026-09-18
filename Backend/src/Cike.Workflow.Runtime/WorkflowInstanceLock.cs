namespace Cike.Workflow.Runtime;

/// <summary>
/// Provides the shared instance-level lock key and timeout for workflow instance operations.
/// Every component that runs or mutates a workflow instance must acquire the lock using
/// <see cref="GetKey" /> so that operations on the same instance are mutually exclusive across nodes.
/// </summary>
public static class WorkflowInstanceLock
{
    /// <summary>
    /// The maximum time to wait for acquiring the instance lock. A run is expected to finish well within this window.
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Returns the distributed lock key for the specified workflow instance.
    /// </summary>
    public static string GetKey(long workflowInstanceId) => $"lock:workflow-instance:{workflowInstanceId}";
}
