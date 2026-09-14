using Cike.AspNetCore.MinimalAPIs.Options;

namespace Cike.Service.Open;

[DependsOn([
    typeof(CikeWorkflowApplicationModule),
    typeof(CikeWorkflowEntityFrameworkCoreModule),
    typeof(CikeAspNetCoreMinimalApiModule),
    typeof(CikeFluentValidationModule),
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

        // HTTP 层支持画布（IActivity 多态）的请求/响应序列化
        context.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new ActivityJsonConverter(context.Services.GetSingletonInstance<IActivityRegistry>()));
        });

        await base.ConfigureServicesAsync(context);


        context.Services.Configure<GlobalMinimalApiRouteOptions>(options =>
        {
            options.Prefix = "api";
            options.Version = "v1";
            options.EnabledAuthorization = false;
        });
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var routeBuilder = context.GetEndpointRouteBuilder();
#if DEBUG
        context.GetApplicationBuilder().UseCikeSwaggerUI("CQRS.Sample");
#endif

        //routeBuilder.MapHub<ChatHub>("/chathub");
        //var jsonOptions = context.ServiceProvider.GetRequiredService<IOptions<JsonOptions>>().Value;
        await base.InitializeAsync(context);
    }
}
