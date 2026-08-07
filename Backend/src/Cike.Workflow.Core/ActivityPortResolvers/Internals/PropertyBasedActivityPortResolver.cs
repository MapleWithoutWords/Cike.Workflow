using Cike.Core.DependencyInjection;
using System.Reflection;

namespace Cike.Workflow.Core.ActivityPortResolvers.Internals;

internal class PropertyBasedActivityPortResolver : IActivityPortResolver, ISingletonDependency
{
    /// <inheritdoc />
    public int Priority => -1;

    /// <inheritdoc />
    public bool GetSupportsActivity(IActivity activity) => true;

    /// <inheritdoc />
    public ValueTask<IEnumerable<ActivityPort>> GetActivityPortsAsync(IActivity activity, CancellationToken cancellationToken = default)
    {
        return new(GetActivityPortsInternal(activity));
    }

    private static IEnumerable<ActivityPort> GetActivityPortsInternal(IActivity activity)
    {
        var activityType = activity.GetType();

        var ports =
            from prop in activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            where typeof(IActivity).IsAssignableFrom(prop.PropertyType) || typeof(IEnumerable<IActivity>).IsAssignableFrom(prop.PropertyType)
            let value = prop.GetValue(activity)
            let isCollection = GetPropertyIsCollection(prop.PropertyType)
            let portName = prop.Name
            where value != null
            select isCollection ? ActivityPort.FromActivities((IEnumerable<IActivity>)value, portName) : ActivityPort.FromActivity((IActivity)value, portName);

        return ports.ToList();
    }

    private static bool GetPropertyIsCollection(Type propertyType) => typeof(IEnumerable<IActivity>).IsAssignableFrom(propertyType);
}
