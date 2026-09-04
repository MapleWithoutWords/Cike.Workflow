namespace Cike.Workflow.Core.WorkflowGraphs.Models;

public record ActivityPort(IActivity? Activity, ICollection<IActivity>? Activities, string PortName)
{
    public static ActivityPort FromActivity(IActivity activity, string portName) => new(activity, null, portName);

    public static ActivityPort FromActivities(IEnumerable<IActivity> activities, string portName) => new(null, activities.ToList(), portName);

    public IEnumerable<IActivity> GetActivities()
    {
        return Activity != null ? [Activity] : Activities!.ToList();
    }
}
