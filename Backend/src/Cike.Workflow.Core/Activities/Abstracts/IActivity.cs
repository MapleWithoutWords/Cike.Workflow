using Cike.Workflow.Core.Activities.FlowchartActivity.Models;
using System.Diagnostics;
using System.Reflection;

namespace Cike.Workflow.Core.Activities.Abstracts;

public interface IActivity
{
    private static readonly string[] CanStartWorkflowPropertyName = ["canStartWorkflow", "CanStartWorkflow"];

    string Id { get; set; }

    /// <summary>
    /// 节点Path
    /// </summary>
    string NodeId { get; set; }

    public string Code { get; set; }

    string? Name { get; set; }

    string Type { get; set; }

    int Version { get; set; }

    IDictionary<string, object> CustomProperties { get; set; }

    IDictionary<string, object> Metadata { get; set; }

    ValueTask<bool> CanExecuteAsync(ActivityExecutionContext context);

    ValueTask ExecuteAsync(ActivityExecutionContext context);

    public IEnumerable<PropertyInfo> GetInputProperties() => GetType().GetProperties().Where(x => typeof(Input).IsAssignableFrom(x.PropertyType)).ToList();

    public TDelegate GetDelegate<TDelegate>(string methodName) where TDelegate : Delegate
    {
        var activityType = GetType();
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        var resumeMethodInfo = default(MethodInfo?);
        var currentType = activityType;

        while (currentType != null && resumeMethodInfo == null)
        {
            resumeMethodInfo = currentType.GetMethod(methodName, bindingFlags);
            currentType = currentType.BaseType;
        }

        if (resumeMethodInfo == null)
            throw new Exception($"Can't find method name {methodName} on type {activityType} or its base type {activityType.BaseType}");

        return resumeMethodInfo.IsStatic ? (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), resumeMethodInfo) : (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), this, resumeMethodInfo);
    }

    public ExecuteActivityDelegate GetResumeActivityDelegate(string resumeMethodName) => GetDelegate<ExecuteActivityDelegate>(resumeMethodName);

    public ActivityCompletionCallback GetActivityCompletionCallback(string completionMethodName) => GetDelegate<ActivityCompletionCallback>(completionMethodName);

    public IEnumerable<(string Name, Output Value)> GetOutputs()
    {
        var outputProps = GetType().GetProperties().Where(x => typeof(Output).IsAssignableFrom(x.PropertyType)).ToList();

        var query =
            from outputProp in outputProps
            let output = (Output?)outputProp.GetValue(this)
            where output != null
            select new Tuple<string, Output>(outputProp.Name, output);

        return query.Select(x => (x.Item1, x.Item2)).ToList();
    }

    public bool GetCanStartWorkflow() => CustomProperties.GetValueOrDefault(CanStartWorkflowPropertyName, () => false);

    /// <summary>
    /// Sets a flag indicating whether this activity can be used for starting a workflow.
    /// </summary>
    public void SetCanStartWorkflow(bool value) => CustomProperties[CanStartWorkflowPropertyName[0]] = value;

    public MergeMode? GetMergeMode()
    {
        if (!CustomProperties.TryGetValue("mergeMode", out var value))
            return null;

        // Handle both string and enum values for backwards compatibility
        var result = value switch
        {
            MergeMode mode => mode,
            string str when Enum.TryParse<MergeMode>(str, true, out var mode) => mode,
            _ => (MergeMode?)null
        };

        // Treat MergeMode.None as equivalent to null (no merge mode set)
        return result == MergeMode.None ? null : result;
    }

    public void SetMergeMode(MergeMode? value)
    {
        // Treat MergeMode.None as equivalent to null (no merge mode set)
        if (value == null || value == MergeMode.None)
            CustomProperties.Remove("mergeMode");
        else
            CustomProperties["mergeMode"] = value.ToString()!;
    }
}
