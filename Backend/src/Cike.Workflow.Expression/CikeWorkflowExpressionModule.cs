using Cike.Core.Modularity;
using Cike.Workflow.Common;
using Cike.Workflow.Expressions.Internals;
using Cike.Workflow.Expressions.LiteralExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Expressions;

[DependsOn([
    typeof(CikeWorkflowCommonModule)
    ])]
public class CikeWorkflowExpressionModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IExpressionDescriptorRegistry>(ExpressionDescriptorRegistry.CreateDefault());
        var expressionDescriptorRegistry = context.Services.GetSingletonInstance<IExpressionDescriptorRegistry>();
        expressionDescriptorRegistry.Add(new ExpressionDescriptor
        {
            Type = "Literal",
            DisplayName = "Literal",
            HandlerFactory = serviceProvider => ActivatorUtilities.GetServiceOrCreateInstance<LiteralExpressionHandler>(serviceProvider)
        });

        await base.ConfigureServicesAsync(context);
    }
}
