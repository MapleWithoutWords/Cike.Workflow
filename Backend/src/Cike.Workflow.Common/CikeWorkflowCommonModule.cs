using Cike.Core.Modularity;
using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Common.Serialization.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Common;

public class CikeWorkflowCommonModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ISerializationTypeRegistry>(SerializationTypeRegistry.CreateDefault());
        await base.ConfigureServicesAsync(context);
    }
}
