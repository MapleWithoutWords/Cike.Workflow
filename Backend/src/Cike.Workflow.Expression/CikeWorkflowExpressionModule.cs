using Cike.Core.Modularity;
using Cike.Workflow.Common;
using Cike.Workflow.Expressions.ExpressionHanlders;
using Cike.Workflow.Expressions.Services;
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
            HandlerFactory = serviceProvider => serviceProvider.GetRequiredService<LiteralExpressionHandler>()
        });

        await base.ConfigureServicesAsync(context);
    }
}
