using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivityDescriptors.Models;

public class TenantRegistryData
{
    public ConcurrentDictionary<(string Type, int Version), ActivityDescriptor> ActivityDescriptors { get; } = new();

    internal ConcurrentDictionary<string, ActivityDescriptor> LatestActivityDescriptors { get; } = new();

    public ConcurrentDictionary<Type, ICollection<ActivityDescriptor>> ProvidedActivityDescriptors { get; } = new();

    public ICollection<ActivityDescriptor> GetOrCreateProviderDescriptors(Type providerType)
    {
        return ProvidedActivityDescriptors.GetOrAdd(providerType, _ => new List<ActivityDescriptor>());
    }

    public void Add(ActivityDescriptor? descriptor, ICollection<ActivityDescriptor> providerDescriptors)
    {
        if (descriptor is null)
        {
            return;
        }

        var activityDescriptors = ActivityDescriptors;
        var descriptorKey = (descriptor.TypeName, descriptor.Version);

        // If the descriptor already exists, replace it. But log a warning.
        if (activityDescriptors.TryGetValue(descriptorKey, out var existingDescriptor))
        {
            // Remove the existing descriptor from the providerDescriptors collection.
            providerDescriptors.Remove(existingDescriptor);
        }

        activityDescriptors[descriptorKey] = descriptor;
        UpdateLatestDescriptor(descriptor);
        providerDescriptors.Add(descriptor);
    }

    public void RemoveDescriptor(ActivityDescriptor descriptor)
    {
        if (!ActivityDescriptors.TryRemove((descriptor.TypeName, descriptor.Version), out var removedDescriptor))
            return;

        if (LatestActivityDescriptors.TryGetValue(removedDescriptor.TypeName, out var latestDescriptor) && latestDescriptor.Version == removedDescriptor.Version)
            RecomputeLatestDescriptor(removedDescriptor.TypeName);
    }

    private void RecomputeLatestDescriptor(string typeName)
    {
        ActivityDescriptor? latestDescriptor = null;
        foreach (var descriptor in ActivityDescriptors.Values)
        {
            if (descriptor.TypeName != typeName)
                continue;

            if (latestDescriptor == null || descriptor.Version > latestDescriptor.Version)
                latestDescriptor = descriptor;
        }

        if (latestDescriptor == null)
            LatestActivityDescriptors.TryRemove(typeName, out _);
        else
            LatestActivityDescriptors[typeName] = latestDescriptor;
    }

    private void UpdateLatestDescriptor(ActivityDescriptor descriptor)
    {
        LatestActivityDescriptors.AddOrUpdate(
            descriptor.TypeName,
            descriptor,
            (_, latestDescriptor) => descriptor.Version >= latestDescriptor.Version ? descriptor : latestDescriptor);
    }
}
