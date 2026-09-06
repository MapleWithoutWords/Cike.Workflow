using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Activities.FlowchartActivity.Models;

public class ActivityConnection : IEquatable<ActivityConnection>
{
    [JsonConstructor]
    public ActivityConnection()
    {
    }

    public ActivityConnection(ActivityEndpoint source, ActivityEndpoint target)
    {
        Source = source;
        Target = target;
    }

    public ActivityConnection(IActivity source, IActivity target)
    {
        Source = new(source.Id);
        Target = new(target.Id);
    }

    public ActivityEndpoint Source { get; set; } = null!;

    public ActivityEndpoint Target { get; set; } = null!;

    public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

    public override string ToString() =>
        $"{Source.ActivityId}{(string.IsNullOrEmpty(Source.Port) ? "" : $":{Source.Port}")}->" +
        $"{Target.ActivityId}{(string.IsNullOrEmpty(Target.Port) ? "" : $":{Target.Port}")}";

    // Implement equality logic
    public bool Equals(ActivityConnection? other)
    {
        if (other == null) return false;
        return AreEndpointsEqual(Source, other.Source) && AreEndpointsEqual(Target, other.Target);
    }

    public override bool Equals(object? obj)
    {
        return obj is ActivityConnection other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetEndpointHashCode(Source), GetEndpointHashCode(Target));
    }

    private static bool AreEndpointsEqual(ActivityEndpoint e1, ActivityEndpoint e2)
    {
        return e1.ActivityId.Equals(e2.ActivityId) && e1.Port == e2.Port;
    }

    private static int GetEndpointHashCode(ActivityEndpoint endpoint)
    {
        return HashCode.Combine(endpoint.ActivityId.GetHashCode(), endpoint.Port?.GetHashCode() ?? 0);
    }
}

public static class ConnectionsExtensions
{
    extension(ICollection<ActivityConnection> connections)
    {
        /// <summary>
        /// Returns all inbound connections of the specified activity.
        /// </summary>
        public IEnumerable<ActivityConnection> InboundConnections(IActivity activity) => connections.Where(x => x.Target.ActivityId == activity.Id).Distinct().ToList();
        public IEnumerable<ActivityConnection> InboundConnections(string activityId) => connections.Where(x => x.Target.ActivityId == activityId).Distinct().ToList();

        /// <summary>
        /// Returns all inbound activities of the specified activity.
        /// </summary>
        public IEnumerable<string> InboundActivities(IActivity activity) => connections.InboundConnections(activity).Select(x => x.Source.ActivityId);

        /// <summary>
        /// Returns all outbound connections of the specified activity.
        /// </summary>
        public IEnumerable<ActivityConnection> OutboundConnections(IActivity activity) => connections.Where(x => x.Source.ActivityId == activity.Id).Distinct().ToList();

        /// <summary>
        /// Returns all outbound connections of the specified activity matching the specified outcomes.
        /// </summary>
        public IEnumerable<ActivityConnection> OutboundConnections(IActivity activity, Outcomes outcomes) => connections.OutboundConnections(activity).Where(c => outcomes.Names.Contains(c.Source.Port));

        /// <summary>
        /// Returns all outbound activities of the specified activity.
        /// </summary>
        public IEnumerable<string> OutboundActivities(IActivity activity) => connections.OutboundConnections(activity).Select(x => x.Source.ActivityId);

        /// <summary>
        /// Returns all outbound activities of the specified activity matching the specified outcomes
        /// </summary>
        public IEnumerable<string> OutboundActivities(IActivity activity, Outcomes outcomes) => connections.OutboundConnections(activity).Where(c => outcomes.Names.Contains(c.Source.Port)).Select(x => x.Source.ActivityId);
    }
}
