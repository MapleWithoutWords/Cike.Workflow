namespace Cike.Workflow.Core;

[DependsOn([typeof(CikeWorkflowExpressionModule), typeof(CikeAuthModule), typeof(CikeUniversalIdModule)])]
public class CikeWorkflowCoreModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IStorageDriverRegistry>(StorageDriverRegistry.CreateDefault());
        var storageDriverRegistry = context.Services.GetSingletonInstance<IStorageDriverRegistry>();
        storageDriverRegistry.Add(new StorageDriverDescriptor
        {
            Type = nameof(WorkflowInstanceStorageDriver),
            DisplayName = "Workflow Instance",
            Factory = serviceProvider => serviceProvider.GetRequiredService<WorkflowInstanceStorageDriver>()
        });
        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        await base.InitializeAsync(context);
    }
}
