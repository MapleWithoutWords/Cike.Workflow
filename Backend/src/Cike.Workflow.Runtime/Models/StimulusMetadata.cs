namespace Cike.Workflow.Runtime.Models;

public class StimulusMetadata
{
    public long? WorkflowInstanceId { get; set; }

    public string? CorrelationId { get; set; }

    public long? ActivityInstanceId { get; set; }

    public long? BookmarkId { get; set; }

    public long? ParentWorkflowInstanceId { get; set; }

    public IDictionary<string, object>? Input { get; set; }

    public IDictionary<string, object>? Properties { get; set; }
}
