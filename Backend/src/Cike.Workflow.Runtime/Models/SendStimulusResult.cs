namespace Cike.Workflow.Runtime.Models;

/// <summary>
/// Represents the result of sending a stimulus the engine.
/// </summary>
public record SendStimulusResult(ICollection<RunWorkflowInstanceResponse> WorkflowInstanceResponses);
