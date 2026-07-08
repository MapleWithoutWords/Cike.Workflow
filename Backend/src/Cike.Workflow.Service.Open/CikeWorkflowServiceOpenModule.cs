namespace Cike.Service.Open;

[DependsOn([
    typeof(CikeWorkflowApplicationModule),
    typeof(CikeWorkflowEntityFrameworkCoreModule),
    typeof(CikeAspNetCoreMinimalApiModule),
    ])]
public class CikeWorkflowServiceOpenModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {

        context.Services.AddCikeSwagger("Cike", options =>
        {
            options.SupportNonNullableReferenceTypes();
            //options.DocumentFilter<PolymorphismDocumentFilter<MessagePlatformBaseJsonConfig, MessagePlatformType>>();
        });
        context.Services.AddValidatorsFromAssembly(typeof(CikeWorkflowApplicationModule).Assembly);
        await base.ConfigureServicesAsync(context);
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var routeBuilder = context.GetEndpointRouteBuilder();

        //routeBuilder.MapHub<ChatHub>("/chathub");
        //var jsonOptions = context.ServiceProvider.GetRequiredService<IOptions<JsonOptions>>().Value;
        await base.InitializeAsync(context);
    }
}
