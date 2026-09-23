using Microsoft.Extensions.Logging;

namespace Cike.Workflow.Expression.Liquid.Internals;

internal class ConfigureLiquidEngine(ISerializationTypeRegistry serializationTypeRegistry, ILogger<ConfigureLiquidEngine> logger) : IScopedDependency
{
    private readonly ISerializationTypeRegistry _serializationTypeRegistry = serializationTypeRegistry;

    public Task HandleAsync(TemplateContext context, CancellationToken cancellationToken)
    {
        var options = context.Options;
        var memberAccessStrategy = options.MemberAccessStrategy;

        memberAccessStrategy.Register<ExpandoObject>();
        memberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((x, name) => x.GetValueAsync(name));
        memberAccessStrategy.Register<ExpandoObject, object>((x, name) => ((IDictionary<string, object>)x!)[name]);
        // JsonElement 直接参与 Liquid 属性下钻：Object/Array 仍以 JsonElement 返回（避免 ExpandoObject 分配），
        // 基本类型转 CLR 让 Fluid 走原生渲染路径（数字/字符串/布尔）。未找到属性返回 null → NilValue。
        memberAccessStrategy.Register<JsonElement, object>((elem, name) =>
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(name, out var prop))
                return null!;
            return prop.ValueKind switch
            {
                JsonValueKind.Object or JsonValueKind.Array => prop,
                JsonValueKind.String => prop.GetString()!,
                JsonValueKind.Number => prop.TryGetInt32(out var i) ? i
                                     : prop.TryGetInt64(out var l) ? (object)l
                                     : prop.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null!,
            };
        });
        memberAccessStrategy.Register<ExpressionExecutionContext, LiquidPropertyAccessor>("Variables", x => new LiquidPropertyAccessor(name => GetVariable(x, name, options)));
        memberAccessStrategy.Register<ExpressionExecutionContext, LiquidPropertyAccessor>("Inputs", x => new LiquidPropertyAccessor(name => GetInput(x, name, options)));

        // Register all variable types.
        // The serialization type registry is shared across modules (the JS module registers Jint exception
        // types), and some types have members Fluid cannot reflect on (e.g. by-ref generic arguments).
        // Skip a type when its registration fails instead of crashing the whole template rendering.
        foreach (var type in _serializationTypeRegistry.ListTypes().Where(x => x is { IsClass: true, ContainsGenericParameters: false }))
        {
            try
            {
                memberAccessStrategy.Register(type);
            }
            catch (ArgumentException e)
            {
                logger.LogWarning(e, "Failed to register type {TypeName} on the Liquid member access strategy. The type will not be accessible in Liquid templates.", type.Name);
            }
        }

        return Task.CompletedTask;
    }

    private Task<FluidValue> GetVariable(ExpressionExecutionContext context, string key, TemplateOptions options)
    {
        var value = GetVariableInScope(context, key);
        return Task.FromResult(value == null ? NilValue.Instance : FluidValue.Create(value, options));
    }

    private Task<FluidValue> GetInput(ExpressionExecutionContext context, string key, TemplateOptions options)
    {
        // First, check if the current activity has inputs
        if (context.TryGetActivityExecutionContext(out var activityExecutionContext) &&
            activityExecutionContext.ActivityInput.TryGetValue(key, out var activityValue))
        {
            return Task.FromResult(activityValue == null ? NilValue.Instance : FluidValue.Create(activityValue, options));
        }

        // Fall back to workflow inputs if activity inputs don't contain the key
        var workflowExecutionContext = context.GetWorkflowExecutionContext();
        var input = workflowExecutionContext.Input.TryGetValue(key, out var workflowValue) ? workflowValue : default;

        return Task.FromResult(input == null ? NilValue.Instance : FluidValue.Create(workflowValue, options));
    }

    private static object? GetVariableInScope(ExpressionExecutionContext context, string variableName)
    {
        var q = from variable in context.EnumerateVariablesInScope()
                where variable.Name == variableName
                where variable.TryGet(context, out _)
                select variable.Get(context);

        return q.FirstOrDefault();
    }
}
