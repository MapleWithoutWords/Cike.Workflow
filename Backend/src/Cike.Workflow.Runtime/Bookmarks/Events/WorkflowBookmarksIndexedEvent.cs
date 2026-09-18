using Cike.Cqrs.Commands;
using Cike.EventBus;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Contexts.Models;

namespace Elsa.Workflows.Runtime.Notifications;

public record WorkflowBookmarksIndexedEvent(
    WorkflowExecutionContext WorkflowExecutionContext,
    ICollection<Bookmark> AddedBookmarks,
    ICollection<Bookmark> RemovedBookmarks,
    ICollection<Bookmark> UnchangedBookmarks) : Event;
