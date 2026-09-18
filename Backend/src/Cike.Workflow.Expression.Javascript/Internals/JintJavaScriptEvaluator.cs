namespace Cike.Workflow.Expression.Javascript.Internals;

public class JintJavaScriptEvaluator(ConfigureJintEngine configureJintEngine, IOptions<JintOptions> scriptOptions, IMemoryCache memoryCache)
    : IJavaScriptEvaluator, IScopedDependency
{
    private readonly JintOptions _jintOptions = scriptOptions.Value;

    [RequiresUnreferencedCode("The Jint library uses reflection and can't be statically analyzed.")]
    public async Task<object?> EvaluateAsync(string expression,
        Type returnType,
        ExpressionExecutionContext context,
        ExpressionEvaluatorOptions? options = null,
        Action<Engine>? configureEngine = null,
        CancellationToken cancellationToken = default)
    {
        var preparedScript = GetOrCreatePrepareScript(expression);
        var engine = await GetConfiguredEngine(configureEngine, context, options, cancellationToken);
        configureJintEngine.ConfigureEngineFunctions(engine, context);
        var result = await ExecuteExpressionAndGetResultAsync(engine, preparedScript, cancellationToken);

        return result.ConvertTo(returnType);
    }

    private async Task<Engine> GetConfiguredEngine(Action<Engine>? configureEngine, ExpressionExecutionContext context, ExpressionEvaluatorOptions? options, CancellationToken cancellationToken)
    {
        options ??= new();

        var engineOptions = new Jint.Options
        {
            ExperimentalFeatures = ExperimentalFeature.TaskInterop
        };

        engineOptions.Interop.ArrayConversion = ArrayConversionMode.Copy;
        engineOptions.Interop.EnumConversion = EnumConversionMode.String;

        engineOptions.AllowClr()
            .SetWrapObjectHandler((engine, target, type) =>
            {
                var instance = ObjectWrapper.Create(engine, target);

                if (ObjectArrayHelper.DetermineIfObjectIsArrayLikeClrCollection(target.GetType()))
                    instance.Prototype = engine.Intrinsics.Array.PrototypeObject;

                return instance;
            })
            .AddObjectConverter(new ByteArrayConverter(), typeof(byte[]))
            .AddObjectConverter(new JsonElementConverter(), typeof(JsonElement));
        ConfigureExecutionConstraints(engineOptions, cancellationToken);

        configureJintEngine.ConfigureTypes(engineOptions, cancellationToken);
        _jintOptions.ConfigureEngineOptionsCallback(engineOptions, context);

        var engine = new Engine(engineOptions);

        configureEngine?.Invoke(engine);
        ConfigureArgumentGetters(engine, options);
        _jintOptions.ConfigureEngineCallback(engine, context);

        return engine;
    }

    private void ConfigureExecutionConstraints(Jint.Options options, CancellationToken cancellationToken)
    {
        // An expression that never returns would otherwise occupy the calling thread forever.
        if (_jintOptions.ExecutionTimeout is { } executionTimeout)
            options.TimeoutInterval(executionTimeout);

        if (_jintOptions.MaxStatements is { } maxStatements)
            options.MaxStatements(maxStatements);

        if (_jintOptions.MemoryLimit is { } memoryLimit)
            options.LimitMemory(memoryLimit);

        if (_jintOptions.MaxRecursionDepth is { } maxRecursionDepth)
            options.LimitRecursion(maxRecursionDepth);

        // Cancelling the workflow should also abort a script that is still running.
        options.CancellationToken(cancellationToken);
    }

    private void ConfigureArgumentGetters(Engine engine, ExpressionEvaluatorOptions options)
    {
        foreach (var argument in options.Arguments)
            engine.SetValue($"get{argument.Key}", (Func<object?>)(() => argument.Value));
    }

    private async Task<object?> ExecuteExpressionAndGetResultAsync(Engine engine, Prepared<Script> preparedScript, CancellationToken cancellationToken)
    {
        // EvaluateAsync awaits a returned promise instead of blocking the calling thread on it, which matters
        // for expressions that await a .NET Task, such as the ones calling getSecret().
        var result = await engine.EvaluateAsync(preparedScript, cancellationToken);
        return result.ToObject();
    }

    private Prepared<Script> GetOrCreatePrepareScript(string expression)
    {
        // The key type keeps these entries distinct from any other consumer of the shared cache, so the
        // expression itself can be used as the key. A cache hit then costs a dictionary lookup rather than
        // a hash of the entire expression plus the allocations needed to render that hash as a string.
        var cacheKey = new ScriptCacheKey(expression);

        // Looking the entry up directly rather than through GetOrCreate keeps the factory closure off the
        // hot path: it is only needed on a miss.
        if (memoryCache.TryGetValue(cacheKey, out Prepared<Script> cachedScript))
            return cachedScript;

        using var entry = memoryCache.CreateEntry(cacheKey);

        if (_jintOptions.ScriptCacheTimeout.HasValue)
            entry.SetSlidingExpiration(_jintOptions.ScriptCacheTimeout.Value);

        var preparedScript = PrepareScript(expression);
        entry.Value = preparedScript;
        return preparedScript;
    }

    private Prepared<Script> PrepareScript(string expression)
    {
        var prepareOptions = new ScriptPreparationOptions
        {
            ParsingOptions = new()
            {
                AllowReturnOutsideFunction = true
            },

            // Collected once per distinct expression, alongside the parse that is already cached, and read by
            // the handlers that would otherwise register a global for every variable, workflow input and
            // activity output in scope.
            CollectReferencedGlobals = true
        };
        return Engine.PrepareScript(expression, options: prepareOptions);
    }

    private readonly record struct ScriptCacheKey(string Expression);
}
