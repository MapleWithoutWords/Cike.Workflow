using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Contexts.Models;

namespace Cike.Workflow.Runtime.Bookmarks.Models;

public record UpdateBookmarksRequest(WorkflowExecutionContext WorkflowExecutionContext, Diff<Bookmark> Diff, string? CorrelationId = default);
