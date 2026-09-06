namespace Cike.Workflow.Core.Activities.FlowchartActivity.Models;

public class ActivityEndpoint
{
    [JsonConstructor]
    public ActivityEndpoint()
    {
    }

    public ActivityEndpoint(string activityId, string? port = null)
    {
        ActivityId = activityId;
        Port = port;
    }

    public string ActivityId { get; set; } = null!;

    public string? Port { get; set; }
}
