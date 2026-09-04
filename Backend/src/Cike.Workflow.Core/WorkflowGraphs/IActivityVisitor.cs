using Cike.Workflow.Core.WorkflowGraphs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.WorkflowGraphs;

public interface IActivityVisitor
{
    /// <summary>
    /// Visits the specified activity and returns a tree structure representing the activity and its descendants.
    /// </summary>
    /// <param name="activity">The activity to visit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tree structure representing the activity and its descendants.</returns>
    Task<ActivityNode> VisitAsync(IActivity activity, CancellationToken cancellationToken = default);

}
