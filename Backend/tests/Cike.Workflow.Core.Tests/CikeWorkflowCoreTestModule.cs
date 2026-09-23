using Cike.Core.Modularity;
using Cike.Uow;
using Cike.Workflow.Expression.Javascript;
using Cike.Workflow.Expression.Liquid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Cike.Workflow.Core.Tests;

[DependsOn([
    typeof(CikeWorkflowCoreModule),
    typeof(CikeWorkflowExpressionLiquidModule),
    typeof(CikeWorkflowExpressionJavascriptModule)
    ])]
internal class CikeWorkflowCoreTestModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddLogging();
        context.Services.AddMemoryCache();
        context.Services.AddSingleton(Substitute.For<IUnitOfWork>());
        await base.ConfigureServicesAsync(context);
    }
}
