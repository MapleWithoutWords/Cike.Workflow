using Cike.Workflow.Expression.Javascript.Extensions;

namespace Cike.Workflow.Expression.Javascript.Options;

public class JintOptions
{
    internal Action<Jint.Options, ExpressionExecutionContext> ConfigureEngineOptionsCallback = (_, _) => { };

    internal Action<Engine, ExpressionExecutionContext> ConfigureEngineCallback = (_, _) => { };

    public TimeSpan? ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public int? MaxStatements { get; set; }

    public long? MemoryLimit { get; set; }

    public int? MaxRecursionDepth { get; set; }

    public TimeSpan? ScriptCacheTimeout { get; set; } = TimeSpan.FromDays(1);

    public JintOptions ConfigureEngineOptions(Action<Jint.Options> configurator)
    {
        ConfigureEngineOptionsCallback += (options, _) => configurator(options);
        return this;
    }

    public JintOptions ConfigureEngineOptions(Action<Jint.Options, ExpressionExecutionContext> configurator)
    {
        ConfigureEngineOptionsCallback += configurator;
        return this;
    }

    public JintOptions ConfigureEngine(Action<Engine, ExpressionExecutionContext> configurator)
    {
        ConfigureEngineCallback += configurator;
        return this;
    }

    public JintOptions ConfigureEngine(Action<Engine> configurator)
    {
        return ConfigureEngine((engine, _) => configurator(engine));
    }

    public JintOptions RegisterType<T>()
    {
        return ConfigureEngine(engine => engine.RegisterType<T>());
    }

    public JintOptions RegisterType(Type type)
    {
        return ConfigureEngine(engine => engine.RegisterType(type));
    }
}
