namespace Cike.Workflow.Core.ActivityDescriptors.Internals
{
    internal class TypedActivityProvider(CikeModuleContainer cikeModuleContainer, IActivityDescriber activityDescriber) : IActivityProvider, IScopedDependency
    {
        private readonly List<ActivityDescriptor> _descriptors = new List<ActivityDescriptor>();

        public async ValueTask<IEnumerable<ActivityDescriptor>> GetDescriptorsAsync(CancellationToken cancellationToken = default)
        {
            if (_descriptors.Any())
            {
                return _descriptors;
            }

            foreach (var item in cikeModuleContainer.ModuleTypes)
            {
                foreach (var itemActivityType in item.Assembly.GetTypes().Where(e => e.IsClass && e.IsAbstract == false && typeof(IActivity).IsAssignableFrom(e)))
                {
                    var descriptor = await activityDescriber.DescribeActivityAsync(itemActivityType, cancellationToken);
                    _descriptors.Add(descriptor);
                }
            }
            return _descriptors;
        }
    }
}
