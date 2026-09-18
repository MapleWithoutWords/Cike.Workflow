using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Runtime.Triggers.Models;

/// <summary>
/// Represents a workflow bound to one or more triggers.
/// </summary>
public record TriggerBoundWorkflow(WorkflowGraph WorkflowGraph, ICollection<TriggerEntity> Triggers);
