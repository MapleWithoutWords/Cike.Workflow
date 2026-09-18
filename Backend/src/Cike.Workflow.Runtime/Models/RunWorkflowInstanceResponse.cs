using Cike.Workflow.Core.Contexts.Models;
using Cike.Workflow.Core.Enums;

namespace Cike.Workflow.Runtime.Models;

public record RunWorkflowInstanceResponse
{
    public long WorkflowInstanceId { get; set; }

    public WorkflowStatus Status { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public ICollection<ActivityIncident> Incidents { get; set; } = new List<ActivityIncident>();

    public IDictionary<string, object>? Output { get; set; }
}
