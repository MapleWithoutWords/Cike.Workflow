namespace Cike.Workflow.Core.StorageDrivers.Models;

public class StorageDriverDescriptor
{
    public string Type { get; init; } = default!;

    public string DisplayName { get; set; } = default!;

    public Func<IServiceProvider, IStorageDriver> Factory { get; set; } = default!;
}
