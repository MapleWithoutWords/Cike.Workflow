using Cike.Core.Modularity;
using Cike.Workflow.Core.StorageDrivers;
using Cike.Workflow.Core.StorageDrivers.Internals;
using Cike.Workflow.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core;

[DependsOn([typeof(CikeWorkflowExpressionModule)])]
public class CikeWorkflowCoreModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IStorageDriverRegistry>(StorageDriverRegistry.CreateDefault());
        await base.ConfigureServicesAsync(context);
    }
}
