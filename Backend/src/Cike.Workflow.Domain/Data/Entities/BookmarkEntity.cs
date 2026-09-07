namespace Cike.Workflow.Domain.Data.Entities;

public class BookmarkEntity : AuditedEntity<long>
{
    public string Name { get; set; } = string.Empty;

    public string Hash { get; set; } = null!;

    public long WorkflowInstanceId { get; set; }

    public long ActivityInstanceId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public object? Payload { get; set; }

    public IDictionary<string, string>? Metadata { get; set; }
}
