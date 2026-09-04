using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.WorkflowGraphs.Internals;

public class ActivityPortRegistry : ISingletonDependency
{
    private record RegisteredPort(string PortName, Func<IActivity, IEnumerable<IActivity>> Accessor);

    private readonly ConcurrentDictionary<Type, List<RegisteredPort>> _portsByType = new();

    /// <summary>高优先级，覆盖基于反射的解析器。</summary>
    public int Priority => 100;

    public bool GetSupportsActivity(IActivity activity) => FindRegistrations(activity.GetType()) is not null;

    /// <summary>
    /// 启动时扫描一批活动类型，注册其端口访问器。
    /// </summary>
    public void ScanAndRegister(IEnumerable<Type> activityTypes)
    {
        foreach (var type in activityTypes)
        {
            if (!typeof(IActivity).IsAssignableFrom(type))
                continue;

            var registrations = CreateRegistrations(type);
            if (registrations.Count == 0)
                continue;

            _portsByType[type] = registrations;
        }
    }

    public ValueTask<IEnumerable<ActivityPort>> GetActivityPortsAsync(IActivity activity, CancellationToken cancellationToken = default)
    {
        var activityType = activity.GetType();
        //预防万一
        if (!_portsByType.ContainsKey(activityType))
        {
            ScanAndRegister([activityType]);
        }
        var registrations = FindRegistrations(activityType);
        if (registrations is null) return new(ArraySegment<ActivityPort>.Empty);

        var ports =
            from reg in registrations
            let children = reg.Accessor(activity).Where(x => x is not null).ToList()
            where children.Count > 0
            select children.Count == 1
                ? ActivityPort.FromActivity(children[0], reg.PortName)
                : ActivityPort.FromActivities(children!, reg.PortName);

        return new(ports.ToList());
    }

    private static List<RegisteredPort> CreateRegistrations(Type activityType)
    {
        var props = activityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var result = new List<RegisteredPort>();

        foreach (var prop in props)
        {
            var portName = prop.Name;
            // 单个 IActivity
            if (typeof(IActivity).IsAssignableFrom(prop.PropertyType))
            {
                var accessor = CompileSingle(activityType, prop);
                result.Add(new RegisteredPort(portName, accessor));
            }
            // 集合 IEnumerable<IActivity>
            else if (typeof(IEnumerable<IActivity>).IsAssignableFrom(prop.PropertyType))
            {
                var accessor = CompileMany(activityType, prop);
                result.Add(new RegisteredPort(portName, accessor));
            }
        }

        return result;
    }

    private static Func<IActivity, IEnumerable<IActivity>> CompileSingle(Type activityType, System.Reflection.PropertyInfo prop)
    {
        var activityParam = System.Linq.Expressions.Expression.Parameter(typeof(IActivity), "a");
        var casted = System.Linq.Expressions.Expression.Convert(activityParam, activityType);
        var propAccess = System.Linq.Expressions.Expression.Property(casted, prop);
        var resultVar = System.Linq.Expressions.Expression.Variable(typeof(IActivity), "r");

        // 修复点 1：确保属性的类型可以安全赋值给 IActivity 变量
        var assign = System.Linq.Expressions.Expression.Assign(resultVar, System.Linq.Expressions.Expression.Convert(propAccess, typeof(IActivity)));

        var body = System.Linq.Expressions.Expression.Block(
            typeof(IEnumerable<IActivity>),
            new[] { resultVar },
            assign,
           System.Linq.Expressions.Expression.Condition(
               System.Linq.Expressions.Expression.Equal(resultVar, System.Linq.Expressions.Expression.Constant(null, typeof(IActivity))),
               System.Linq.Expressions.Expression.Constant(ArraySegment<IActivity>.Empty, typeof(IEnumerable<IActivity>)),
               System.Linq.Expressions.Expression.Convert(
                   System.Linq.Expressions.Expression.NewArrayInit(typeof(IActivity), resultVar),
                    typeof(IEnumerable<IActivity>)
                )
            )
        );

        return System.Linq.Expressions.Expression.Lambda<Func<IActivity, IEnumerable<IActivity>>>(body, activityParam).Compile();
    }

    private static Func<IActivity, IEnumerable<IActivity>> CompileMany(Type activityType, System.Reflection.PropertyInfo prop)
    {
        // (IActivity a) => ((IEnumerable<IActivity>?)((T)a).Prop) ?? Empty
        var activityParam = System.Linq.Expressions.Expression.Parameter(typeof(IActivity), "a");
        var casted = System.Linq.Expressions.Expression.Convert(activityParam, activityType);
        var propAccess = System.Linq.Expressions.Expression.Property(casted, prop);
        var enumerableType = typeof(IEnumerable<IActivity>);
        var body = System.Linq.Expressions.Expression.Coalesce(
           System.Linq.Expressions.Expression.Convert(propAccess, enumerableType),
           System.Linq.Expressions.Expression.Constant(ArraySegment<IActivity>.Empty, enumerableType)
        );

        return System.Linq.Expressions.Expression.Lambda<Func<IActivity, IEnumerable<IActivity>>>(body, activityParam).Compile();
    }

    private List<RegisteredPort>? FindRegistrations(Type activityType)
    {
        // 精确匹配
        if (_portsByType.TryGetValue(activityType, out var exact))
            return exact;

        // 兼容子类：找到第一个可分配的注册
        var assignable = _portsByType
            .Where(kv => kv.Key.IsAssignableFrom(activityType))
            .Select(kv => kv.Value)
            .FirstOrDefault();

        return assignable;
    }
}
