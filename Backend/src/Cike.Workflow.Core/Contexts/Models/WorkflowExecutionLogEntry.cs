namespace Cike.Workflow.Core.Contexts.Models;

/// <summary>
/// Represents a workflow execution log entry.
/// </summary>
public record WorkflowExecutionLogEntry(
    long ActivityInstanceId,
    long? ParentActivityInstanceId,
    string ActivityId,
    string ActivityType,
    int ActivityTypeVersion,
    string? ActivityName,
    string NodeId,
    long WorkflowInstanceId,
    DateTime Timestamp,
    string? EventName,
    string? Message,
    string? Payload,
    string LogLevel);
