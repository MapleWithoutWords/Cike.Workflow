using Cike.Workflow.Expressions.Exceptions;

namespace Cike.Workflow.Core.Activities.FlowchartActivity.Models;

/// <summary>
/// Represents a directed graph structure for managing workflow connections.
/// Caches forward and backward connections to optimize graph traversal.
/// </summary>
public class FlowGraph(ICollection<ActivityConnection> connections, IActivity? rootActivity)
{
    private List<ActivityConnection>? _cachedForwardConnections;
    private readonly Dictionary<string, List<ActivityConnection>> _cachedInboundForwardConnections = new();
    private readonly Dictionary<string, List<ActivityConnection>> _cachedInboundConnections = new();
    private readonly Dictionary<string, List<ActivityConnection>> _cachedOutboundConnections = new();
    private readonly Dictionary<ActivityConnection, (bool IsBackwardConnection, bool IsValid)> _cachedIsBackwardConnection = new();
    private readonly Dictionary<string, bool> _cachedIsDanglingActivity = new();
    private readonly Dictionary<string, List<string>> _cachedAncestors = new();

    /// <summary>
    /// Gets the list of forward connections, computing them if not already cached.
    /// </summary>
    private List<ActivityConnection> ForwardConnections => _cachedForwardConnections ??= rootActivity == null ? new() : GetForwardConnections(connections, rootActivity);

    /// <summary>
    /// Retrieves all inbound forward connections for a given activity.
    /// </summary>
    public List<ActivityConnection> GetForwardInboundConnections(IActivity activity) => _cachedInboundForwardConnections.GetOrAdd(activity.Id, () => ForwardConnections.InboundConnections(activity).ToList());

    public List<ActivityConnection> GetForwardInboundConnections(string activityId) => _cachedInboundForwardConnections.GetOrAdd(activityId, () => ForwardConnections.InboundConnections(activityId).ToList());

    /// <summary>
    /// Retrieves all outbound connections for a given activity.
    /// </summary>
    public List<ActivityConnection> GetOutboundConnections(IActivity activity) => _cachedOutboundConnections.GetOrAdd(activity.Id, () => connections.OutboundConnections(activity).ToList());

    /// <summary>
    /// Retrieves all inbound connections for a given activity.
    /// </summary>
    public List<ActivityConnection> GetInboundConnections(IActivity activity) => _cachedInboundConnections.GetOrAdd(activity.Id, () => connections.InboundConnections(activity).ToList());

    /// <summary>
    /// Determines if a given activity is "dangling," meaning it does not exist as a target in any forward connection.
    /// </summary>
    public bool IsDanglingActivity(IActivity activity) => _cachedIsDanglingActivity.GetOrAdd(activity.Id, () => activity != rootActivity && ForwardConnections.All(c => c.Target.ActivityId != activity.Id));

    /// <summary>
    /// Determines if a given connection is a backward connection (i.e., not part of the forward traversal) and whether it is valid.
    /// </summary>
    public bool IsBackwardConnection(ActivityConnection connection, out bool isValid)
    {
        // Check if result is already cached
        if (_cachedIsBackwardConnection.TryGetValue(connection, out var result))
        {
            isValid = result.IsValid;
            return result.IsBackwardConnection;
        }

        // Compute if the connection is backward
        bool isBackwardConnection = !GetForwardInboundConnections(connection.Target.ActivityId).Contains(connection);

        // Compute if the backward connection is valid
        isValid = isBackwardConnection && IsValidBackwardConnection(ForwardConnections, rootActivity, connection);

        // Cache the result
        _cachedIsBackwardConnection[connection] = (isBackwardConnection, isValid);

        return isBackwardConnection;
    }

    /// <summary>
    /// Retrieves all ancestor activities for a given activity by traversing ForwardConnections in reverse.
    /// </summary>
    public List<string> GetAncestorActivities(IActivity activity)
    {
        return _cachedAncestors.GetOrAdd(activity.Id, () => ComputeAncestors(activity));
    }

    /// <summary>
    /// Computes the list of ancestors by following Source activities in ForwardConnections.
    /// </summary>
    private List<string> ComputeAncestors(IActivity activity)
    {
        HashSet<string> ancestors = new();
        Queue<string> queue = new();

        // Find all connections where this activity is the target
        foreach (var connection in ForwardConnections.Where(c => c.Target.ActivityId == activity.Id))
        {
            if (ancestors.Add(connection.Source.ActivityId))
                queue.Enqueue(connection.Source.ActivityId);
        }

        // Traverse upwards through the graph
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var connection in ForwardConnections.Where(c => c.Target.ActivityId == current))
            {
                if (ancestors.Add(connection.Source.ActivityId))
                    queue.Enqueue(connection.Source.ActivityId);
            }
        }

        return ancestors.ToList();
    }

    /// <summary>
    /// Computes the list of forward connections in the graph, excluding cyclic connections.
    /// </summary>
    private static List<ActivityConnection> GetForwardConnections(ICollection<ActivityConnection> connections, IActivity root)
    {
        Dictionary<string, List<string>> adjList = new();

        foreach (var conn in connections)
        {
            if (!adjList.ContainsKey(conn.Source.ActivityId))
                adjList[conn.Source.ActivityId] = new();

            adjList[conn.Source.ActivityId].Add(conn.Target.ActivityId);
        }

        HashSet<string> visited = new();
        HashSet<(string, string)> visitedEdges = new();
        List<(string Source, string Target)> validEdges = new();
        Queue<string> queue = new();

        queue.Enqueue(root.Id);

        while (queue.Count > 0)
        {
            var source = queue.Dequeue();
            visited.Add(source);

            if (!adjList.ContainsKey(source)) continue;

            foreach (var target in adjList[source])
            {
                var edge = (source, target);
                if (visitedEdges.Contains(edge))
                    continue;

                if (HasPathToActivity(validEdges, target, source))
                    continue;

                visitedEdges.Add(edge);
                validEdges.Add((source, target));

                if (!visited.Contains(target))
                    queue.Enqueue(target);
            }
        }

        return validEdges
            .SelectMany(e => connections.Where(c => c.Source.ActivityId == e.Source && c.Target.ActivityId == e.Target))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Determines if there is an existing path from the source activity to the target activity.
    /// Helps in detecting cyclic connections.
    /// </summary>
    private static bool HasPathToActivity(ICollection<(string Source, string Target)> edges, string source, string target)
    {
        if (source == target)
            return true;

        HashSet<string> visited = new();
        Stack<string> stack = new();
        stack.Push(source);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current == target)
                return true;

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            foreach (var next in edges.Where(x => x.Source == current).Select(e => e.Target))
            {
                if (!visited.Contains(next))
                    stack.Push(next);
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a backward connection is valid by ensuring all paths from source to root pass through target.
    /// </summary>
    private static bool IsValidBackwardConnection(List<ActivityConnection> forwardConnections, IActivity? root, ActivityConnection connection)
    {
        if (root == null) return false;

        var pathsToRoot = GetPathsToRoot(forwardConnections, root, connection.Source.ActivityId);

        foreach (var path in pathsToRoot)
        {
            if (!path.Contains(connection.Target.ActivityId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Finds all paths from a given start activity to the root using BFS.
    /// </summary>
    private static List<List<string>> GetPathsToRoot(List<ActivityConnection> forwardConnections, IActivity root, string start)
    {
        List<List<string>> paths = new();
        Queue<List<string>> queue = new();
        queue.Enqueue(new()
            { start });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var lastNode = path.Last();

            if (lastNode == root.Id)
            {
                paths.Add([.. path]);
                continue;
            }

            var previousNodes = forwardConnections
                .Where(c => c.Target.ActivityId == lastNode)
                .Select(c => c.Source.ActivityId);

            foreach (var prev in previousNodes)
            {
                if (!path.Contains(prev))
                {
                    var newPath = new List<string>(path) { prev };
                    queue.Enqueue(newPath);
                }
            }
        }

        return paths;
    }
}
