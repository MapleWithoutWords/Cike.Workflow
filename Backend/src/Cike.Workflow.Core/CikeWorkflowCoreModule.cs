namespace Cike.Workflow.Core;

[DependsOn([typeof(CikeWorkflowExpressionModule), typeof(CikeAuthModule)])]
public class CikeWorkflowCoreModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IStorageDriverRegistry>(StorageDriverRegistry.CreateDefault());
        await base.ConfigureServicesAsync(context);
    }
}
