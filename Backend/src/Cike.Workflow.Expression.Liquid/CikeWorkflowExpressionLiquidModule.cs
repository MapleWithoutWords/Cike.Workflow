namespace Cike.Workflow.Expression.Liquid;

[DependsOn([
    typeof(CikeWorkflowExpressionModule)
    ])]
public class CikeWorkflowExpressionLiquidModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<FluidOptions>(options =>
        {
            options.ConfigureFilters = context => context.Options.Filters
                .WithArrayFilters()
                .WithStringFilters()
                .WithNumberFilters()
                .WithMiscFilters();
            options.FilterRegistrations["base64"] = typeof(Base64Filter);
            options.FilterRegistrations["keys"] = typeof(DictionaryKeysFilter);
        });
        var expressionDescriptorRegistry = context.Services.GetSingletonInstance<IExpressionDescriptorRegistry>();
        expressionDescriptorRegistry.Add(new ExpressionDescriptor
        {
            Type = "Liquid",
            DisplayName = "Liquid 表达式",
            HandlerFactory = serviceProvider => ActivatorUtilities.GetServiceOrCreateInstance<LiquidExpressionHandler>(serviceProvider)
        });
        await base.ConfigureServicesAsync(context);
    }
}
