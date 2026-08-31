namespace Cike.Workflow.Core.ActivityDescriptors.Internals;

internal class ActivityRegistry(ICurrentTenantAccessor currentTenantAccessor, ILogger<ActivityRegistry> logger) : IActivityRegistry, ISingletonDependency
{
    private readonly ConcurrentDictionary<Guid, TenantRegistryData> _tenantRegistries = new();
    private readonly TenantRegistryData _agnosticRegistry = new();

    private readonly ConcurrentDictionary<Type, byte> _initializedProviders = new();
    private readonly ConcurrentDictionary<Type, SemaphoreSlim> _providerInitializationLocks = new();

    public async Task RefreshDescriptorsAsync(IEnumerable<IActivityProvider> activityProviders, CancellationToken cancellationToken = default)
    {
        foreach (var item in activityProviders)
            await RefreshDescriptorsAsync(item, cancellationToken);
    }

    public async Task RefreshDescriptorsAsync(IActivityProvider activityProvider, CancellationToken cancellationToken = default)
    {
        var providerType = activityProvider.GetType();

        var descriptors = (await activityProvider.GetDescriptorsAsync(cancellationToken)).ToList();

        var descriptorsByTenant = descriptors.GroupBy(d => d.TenantId ?? Guid.Empty);

        foreach (var group in descriptorsByTenant)
        {
            var tenantId = group.Key;
            var registry = GetOrCreateRegistry(tenantId);

            if (registry.ProvidedActivityDescriptors.TryGetValue(providerType, out var oldDescriptors))
            {
                foreach (var oldDescriptor in oldDescriptors.ToList())
                {
                    registry.RemoveDescriptor(oldDescriptor);
                }
            }

            var providerDescriptors = new List<ActivityDescriptor>();
            foreach (var descriptor in group)
            {
                registry.Add(descriptor, providerDescriptors);
            }

            registry.ProvidedActivityDescriptors[providerType] = providerDescriptors;
        }
    }

    public async Task EnsureDescriptorsAsync(IActivityProvider activityProvider, CancellationToken cancellationToken = default)
    {
        if (activityProvider is not ITenantAgnosticActivityProvider)
        {
            await RefreshDescriptorsAsync(activityProvider, cancellationToken);
            return;
        }

        var providerType = activityProvider.GetType();
        if (_initializedProviders.ContainsKey(providerType))
            return;

        var initializationLock = _providerInitializationLocks.GetOrAdd(providerType, _ => new(1, 1));
        await initializationLock.WaitAsync(cancellationToken);

        try
        {
            if (_initializedProviders.ContainsKey(providerType))
                return;

            await RefreshDescriptorsAsync(activityProvider, cancellationToken);
            _initializedProviders.TryAdd(providerType, 0);
        }
        finally
        {
            initializationLock.Release();
        }
    }

    public IEnumerable<ActivityDescriptor> ListAll()
    {
        var currentTenantId = currentTenantAccessor.GetTenantId() ?? Guid.Empty;

        var tenantDescriptors = _tenantRegistries.TryGetValue(currentTenantId, out var tenantRegistry)
            ? tenantRegistry.ActivityDescriptors.Values
            : Enumerable.Empty<ActivityDescriptor>();

        var agnosticDescriptors = _agnosticRegistry.ActivityDescriptors.Values;

        return tenantDescriptors.Concat(agnosticDescriptors);
    }

    public IEnumerable<ActivityDescriptor> ListByProvider(Type providerType)
    {
        var currentTenantId = currentTenantAccessor.GetTenantId() ?? Guid.Empty;

        var tenantDescriptors = _tenantRegistries.TryGetValue(currentTenantId, out var tenantRegistry) &&
                                tenantRegistry.ProvidedActivityDescriptors.TryGetValue(providerType, out var tenantProviderDescriptors)
            ? tenantProviderDescriptors
            : Enumerable.Empty<ActivityDescriptor>();

        var agnosticDescriptors = _agnosticRegistry.ProvidedActivityDescriptors.TryGetValue(providerType, out var agnosticProviderDescriptors)
            ? agnosticProviderDescriptors
            : Enumerable.Empty<ActivityDescriptor>();

        return tenantDescriptors.Concat(agnosticDescriptors);
    }

    public ActivityDescriptor? Find(string type)
    {
        var currentTenantId = currentTenantAccessor.GetTenantId() ?? Guid.Empty;

        if (_tenantRegistries.TryGetValue(currentTenantId, out var tenantRegistry))
        {
            if (tenantRegistry.LatestActivityDescriptors.TryGetValue(type, out var tenantDescriptor))
                return tenantDescriptor;
        }

        return _agnosticRegistry.LatestActivityDescriptors.TryGetValue(type, out var agnosticDescriptor)
            ? agnosticDescriptor
            : null;
    }

    /// <inheritdoc />
    public ActivityDescriptor? Find(string type, int version)
    {
        var currentTenantId = currentTenantAccessor.GetTenantId() ?? Guid.Empty;

        if (_tenantRegistries.TryGetValue(currentTenantId, out var tenantRegistry) &&
            tenantRegistry.ActivityDescriptors.TryGetValue((type, version), out var tenantDescriptor))
        {
            return tenantDescriptor;
        }

        return _agnosticRegistry.ActivityDescriptors.TryGetValue((type, version), out var agnosticDescriptor)
            ? agnosticDescriptor
            : null;
    }

    public ActivityDescriptor? Find(Func<ActivityDescriptor, bool> predicate)
    {
        var currentTenantId = currentTenantAccessor.GetTenantId() ?? Guid.Empty;

        if (_tenantRegistries.TryGetValue(currentTenantId, out var tenantRegistry))
        {
            var tenantMatch = tenantRegistry.ActivityDescriptors.Values.FirstOrDefault(predicate);
            if (tenantMatch != null) return tenantMatch;
        }

        return _agnosticRegistry.ActivityDescriptors.Values.FirstOrDefault(predicate);
    }

    public IEnumerable<ActivityDescriptor> FindMany(Func<ActivityDescriptor, bool> predicate)
    {
        var currentTenantId = currentTenantAccessor.GetTenantId() ?? Guid.Empty;

        var tenantDescriptors = _tenantRegistries.TryGetValue(currentTenantId, out var tenantRegistry)
            ? tenantRegistry.ActivityDescriptors.Values.Where(predicate)
            : Enumerable.Empty<ActivityDescriptor>();

        var agnosticDescriptors = _agnosticRegistry.ActivityDescriptors.Values.Where(predicate);

        return tenantDescriptors.Concat(agnosticDescriptors);
    }

    public void Add(Type providerType, ActivityDescriptor descriptor)
    {
        var registry = GetOrCreateRegistry(descriptor.TenantId);
        var providerDescriptors = registry.GetOrCreateProviderDescriptors(providerType);

        registry.Add(descriptor, providerDescriptors);
    }

    public void Remove(Type providerType, ActivityDescriptor descriptor)
    {
        var registry = GetOrCreateRegistry(descriptor.TenantId);
        if (registry.ProvidedActivityDescriptors.TryGetValue(providerType, out var providerDescriptors))
        {
            providerDescriptors.Remove(descriptor);
            registry.RemoveDescriptor(descriptor);
        }
    }

    private TenantRegistryData GetOrCreateRegistry(Guid? tenantId)
    {
        // Null or agnostic tenant ID goes to agnostic registry
        if (tenantId == null || tenantId == Guid.Empty)
            return _agnosticRegistry;

        // Get or create tenant-specific registry
        return _tenantRegistries.GetOrAdd(tenantId.Value, _ => new());
    }
}
