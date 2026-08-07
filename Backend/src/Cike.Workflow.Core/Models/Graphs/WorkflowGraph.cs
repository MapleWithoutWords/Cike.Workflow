using Cike.Workflow.Core.Activities;
using System.Security.Cryptography;
using System.Text;

namespace Cike.Workflow.Core.Models.Graphs;

public record WorkflowGraph
{
    public WorkflowGraph(WorkflowActivity workflow, ActivityNode root, IEnumerable<ActivityNode> nodes)
    {
        Workflow = workflow;
        Root = root;
        Nodes = nodes.ToList();
        NodeIdLookup = Nodes.ToDictionary(x => x.NodeId);
        NodeActivityLookup = Nodes.ToDictionary(x => x.Activity);
    }

    public WorkflowActivity Workflow { get; }

    public ActivityNode Root { get; }

    public ICollection<ActivityNode> Nodes { get; }

    public IDictionary<IActivity, ActivityNode> NodeActivityLookup { get; }

    public IDictionary<string, ActivityNode> NodeIdLookup { get; }

    public IActivity? FindActivity(ActivityHandle handle)
    {
        return handle.ActivityId != null
            ? FindActivityById(handle.ActivityId)
            : handle.ActivityNodeId != null
                ? FindActivityByNodeId(handle.ActivityNodeId)
                    : null;
    }

    public ActivityNode? FindNodeById(string nodeId) => NodeIdLookup.TryGetValue(nodeId, out var node) ? node : null;

    public ActivityNode? FindNodeByActivity(IActivity activity)
    {
        return NodeActivityLookup.TryGetValue(activity, out var node) ? node : null;
    }

    public ActivityNode? FindNodeByActivityId(string activityId) => Nodes.FirstOrDefault(x => x.Activity.Id == activityId);

    public IActivity? FindActivityByNodeId(string nodeId) => FindNodeById(nodeId)?.Activity;

    public IActivity? FindActivityById(string activityId) => FindNodeById(NodeIdLookup.SingleOrDefault(n => n.Key.EndsWith(activityId)).Value.NodeId)?.Activity;
}
