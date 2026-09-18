namespace Cike.Workflow.Runtime.Bookmarks.Models;

public class ResumeBookmarkRequest
{
    public long WorkflowInstanceId { get; set; }

    public long BookmarkId { get; set; }

    public long? ActivityInstanceId { get; set; }

    public IDictionary<string, object>? Properties { get; set; }

    public IDictionary<string, object>? Input { get; set; }
}
