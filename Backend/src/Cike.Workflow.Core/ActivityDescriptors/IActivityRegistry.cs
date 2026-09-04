namespace Cike.Workflow.Core.ActivityDescriptors;

public interface IActivityRegistry
{
    Task RefreshDescriptorsAsync(IEnumerable<IActivityProvider> activityProviders, CancellationToken cancellationToken = default);

    Task RefreshDescriptorsAsync(IActivityProvider activityProvider, CancellationToken cancellationToken = default);

    Task EnsureDescriptorsAsync(IActivityProvider activityProvider, CancellationToken cancellationToken = default) =>
        RefreshDescriptorsAsync(activityProvider, cancellationToken);

    IEnumerable<ActivityDescriptor> ListAll();

    IEnumerable<ActivityDescriptor> ListByProvider(Type providerType);

    ActivityDescriptor? Find(string type);

    ActivityDescriptor? Find(string type, int version);

    ActivityDescriptor? Find(Func<ActivityDescriptor, bool> predicate);

    IEnumerable<ActivityDescriptor> FindMany(Func<ActivityDescriptor, bool> predicate);

    void Add(Type providerType, ActivityDescriptor descriptor);

    void Remove(Type providerType, ActivityDescriptor descriptor);

    Task RegisterAsync(IEnumerable<Type> activityTypes, CancellationToken cancellationToken = default);
}
