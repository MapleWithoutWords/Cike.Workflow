namespace Cike.Workflow.Expression.Liquid.Internals;


internal class LiquidTemplateManager : ILiquidTemplateManager, IScopedDependency
{
    private readonly LiquidParser _parser;
    private readonly IMemoryCache _memoryCache;
    private readonly FluidOptions _options;
    private readonly ConfigureLiquidEngine _configureLiquidEngine;

    /// <summary>
    /// Constructor.
    /// </summary>
    public LiquidTemplateManager(LiquidParser parser, IMemoryCache memoryCache, IOptions<FluidOptions> options, ConfigureLiquidEngine configureLiquidEngine)
    {
        _parser = parser;
        _memoryCache = memoryCache;
        _options = options.Value;
        _configureLiquidEngine = configureLiquidEngine;
    }

    /// <inheritdoc />
    public async Task<string?> RenderAsync(string template, ExpressionExecutionContext expressionExecutionContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(template))
            return default!;

        var result = GetCachedTemplate(template);
        var templateContext = await CreateTemplateContextAsync(expressionExecutionContext, cancellationToken);
        var encoder = _options.Encoder;
        AddFilters(templateContext, _options, expressionExecutionContext.ServiceProvider);

        return await result.RenderAsync(templateContext, encoder);
    }

    private IFluidTemplate GetCachedTemplate(string source)
    {
        var result = _memoryCache.GetOrCreate(
            source,
            e =>
            {
                if (!TryParse(source, out var parsed, out var error))
                {
                    error = "{% raw %}\n" + error + "\n{% endraw %}";
                    TryParse(error, out parsed, out error);

                    e.SetSlidingExpiration(TimeSpan.FromMilliseconds(100));
                    return parsed;
                }

                // TODO: add signal based cache invalidation.
                e.SetSlidingExpiration(TimeSpan.FromSeconds(30));
                return parsed;
            });
        return result!;
    }

    /// <inheritdoc />
    public bool Validate(string template, out string error) => TryParse(template, out _, out error);

    private bool TryParse(string template, out IFluidTemplate result, out string error) => _parser.TryParse(template, out result, out error);

    private async Task<TemplateContext> CreateTemplateContextAsync(ExpressionExecutionContext expressionExecutionContext, CancellationToken cancellationToken)
    {
        var context = new TemplateContext(expressionExecutionContext, new TemplateOptions());
        await _configureLiquidEngine.HandleAsync(context, cancellationToken);
        context.SetValue("ExpressionExecutionContext", expressionExecutionContext);
        return context;
    }

    void AddFilters(TemplateContext templateContext, FluidOptions options, IServiceProvider services)
    {
        foreach (var (key, value) in options.FilterRegistrations)
        {
            templateContext.Options.Filters.AddFilter(key, (input, arguments, ctx) =>
            {
                var filter = (ILiquidFilter)services.GetRequiredService(value);
                return filter.ProcessAsync(input, arguments, ctx);
            });
        }
        options.ConfigureFilters(templateContext);
    }
}
