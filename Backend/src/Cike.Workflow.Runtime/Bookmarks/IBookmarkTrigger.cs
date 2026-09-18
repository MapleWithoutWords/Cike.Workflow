using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Runtime.Bookmarks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Bookmarks;

public interface IBookmarkTrigger
{
    Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync<TActivity>(object stimulus, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default) where TActivity : IActivity;

    Task<RunWorkflowInstanceResponse?> ResumeAsync(long bookmarkId, IDictionary<string, object> input, CancellationToken cancellationToken = default);

    Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync<TActivity>(object stimulus, long? workflowInstanceId, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default) where TActivity : IActivity;

    Task<RunWorkflowInstanceResponse?> ResumeAsync<TActivity>(long bookmarkId, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default) where TActivity : IActivity;

    Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync(ResumeBookmarkRequest request, CancellationToken cancellationToken = default);

    Task<IEnumerable<RunWorkflowInstanceResponse>> ResumeAsync(BookmarkFilter filter, ResumeBookmarkOptions? options = null, CancellationToken cancellationToken = default);
}
