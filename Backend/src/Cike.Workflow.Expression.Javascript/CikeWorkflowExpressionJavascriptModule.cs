namespace Cike.Workflow.Expression.Javascript;

[DependsOn([
    typeof(CikeWorkflowExpressionModule)
    ])]
public class CikeWorkflowExpressionJavascriptModule : CikeModule
{
    private static List<string> javascriptLibraries = new(["lodash"]);

    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.Configure<JintOptions>(options =>
        {
            options.ScriptCacheTimeout = TimeSpan.FromDays(1);

            options.ConfigureEngine((engine, context) =>
            {
                foreach (var moduleName in javascriptLibraries)
                {
                    // Embedded resource names carry the RootNamespace prefix (csproj: EmbeddedResource ClientLib/dist/*.js);
                    // derive from the module namespace so the name survives assembly/namespace renames.
                    var resourceName = $"{typeof(CikeWorkflowExpressionJavascriptModule).Namespace}.ClientLib.dist.{moduleName}.js";
                    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!;
                    using var reader = new StreamReader(stream);
                    var script = reader.ReadToEnd();
                    engine.Execute(script);
                }
            });
        });

        context.Services.Configure<SerializationTypeOptions>(options =>
        {
            options.RegisterTypeAlias(typeof(ScriptPreparationException), nameof(ScriptPreparationException));
            options.RegisterTypeAlias(typeof(JavaScriptException), nameof(JavaScriptException));
            options.RegisterTypeAlias(typeof(SyntaxErrorException), nameof(SyntaxErrorException));

            var wrapperExceptionType = typeof(JavaScriptException).GetNestedType("JavaScriptErrorWrapperException", BindingFlags.Public | BindingFlags.NonPublic);
            if (wrapperExceptionType != null)
                options.RegisterTypeAlias(wrapperExceptionType, "Jint.JavaScriptErrorWrapperException");
        });

        var expressionDescriptorRegistry = context.Services.GetSingletonInstance<IExpressionDescriptorRegistry>();
        expressionDescriptorRegistry.Add(new ExpressionDescriptor
        {
            Type = "JavaScript",
            DisplayName = "JavaScript 表达式",
            Icon = "code",
            HandlerFactory = ActivatorUtilities.GetServiceOrCreateInstance<JavaScriptExpressionHandler>
        });

        await base.ConfigureServicesAsync(context);
    }
}
