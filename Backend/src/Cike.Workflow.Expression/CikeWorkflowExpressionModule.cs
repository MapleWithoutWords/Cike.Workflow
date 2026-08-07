using Cike.Core.Modularity;
using Cike.Workflow.Expressions.ExpressionHanlders;
using Cike.Workflow.Expressions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Expressions;

public class CikeWorkflowExpressionModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<ExpressionOptions>(options => { });

        context.Services.AddSingleton<IExpressionDescriptorRegistry>(ExpressionDescriptorRegistry.CreateDefault());
        var expressionDescriptorRegistry = context.Services.GetSingletonInstance<IExpressionDescriptorRegistry>();
        expressionDescriptorRegistry.Add(new ExpressionDescriptor
        {
            Type = "Literal",
            DisplayName = "Name",
            HandlerFactory = serviceProvider => serviceProvider.GetRequiredService<LiteralExpressionHandler>()
        });

        await base.ConfigureServicesAsync(context);
    }
}
