using Cike.Workflow.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Bookmarks;

public interface IBookmarkQueue
{
    Task EnqueueAsync(long workflowInstanceId, long bookmarkId, string correlationId, string stimulusHash, long activityInstanceId, ResumeBookmarkOptionsValueObject options, CancellationToken cancellationToken = default);
}
