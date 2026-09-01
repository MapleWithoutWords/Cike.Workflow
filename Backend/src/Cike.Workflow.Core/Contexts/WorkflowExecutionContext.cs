using Cike.Core.Extensions.System;
using Cike.Workflow.Core.ActivityDescriptors;
using Cike.Workflow.Core.ActivityDescriptors.Internals;
using Cike.Workflow.Core.ActivitySchedulers;
using Cike.Workflow.Core.ActivitySchedulers.Models;
using Cike.Workflow.Core.Contexts.Models;
using Cike.Workflow.Core.Helpers;
using Cike.Workflow.Core.Variables;
using System.Text.Json;
using System.Xml.Linq;

namespace Cike.Workflow.Core.Contexts;

public record ActivityCompletionCallbackEntry(ActivityExecutionContext Owner, ActivityNode Child, ActivityCompletionCallback? CompletionCallback);

public class WorkflowExecutionContext : IExecutionContext
{
    private ICollection<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();
    private ICollection<CancellationTokenRegistration> _cancellationRegistrations = new List<CancellationTokenRegistration>();
    private IList<ActivityExecutionContext> _activityExecutionContexts;
    private readonly IList<ActivityCompletionCallbackEntry> _completionCallbackEntries = new List<ActivityCompletionCallbackEntry>();

    private WorkflowExecutionContext(
        IServiceProvider serviceProvider,
        WorkflowGraph workflowGraph,
        long id,
        string correlationId,
        long? parentWorkflowInstanceId,
        IDictionary<string, object>? input,
        IDictionary<string, object>? properties,
        ExecuteActivityDelegate? executeDelegate,
        string? triggerActivityId,
        IEnumerable<Bookmark> originalBookmarks,
        DateTime createdAt,
        CancellationToken cancellationToken)
    {
        ServiceProvider = serviceProvider;
        ActivityRegistry = serviceProvider.GetRequiredService<IActivityRegistry>();
        ActivityRegistryLookup = serviceProvider.GetRequiredService<IActivityRegistryLookupService>();
        Scheduler = serviceProvider.GetRequiredService<IActivitySchedulerFactory>().CreateScheduler();

        Status = WorkflowStatus.Pending;
        Id = id;
        CorrelationId = correlationId;
        ParentWorkflowInstanceId = parentWorkflowInstanceId;
        _activityExecutionContexts = new List<ActivityExecutionContext>();
        Input = input != null ? new(input, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        Properties = properties != null ? new(properties, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        ExecuteDelegate = executeDelegate;
        TriggerActivityId = triggerActivityId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        CancellationToken = cancellationToken;
        OriginalBookmarks = originalBookmarks.ToList();
        WorkflowGraph = workflowGraph;
        var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cancellationTokenSources.Add(linkedCancellationTokenSource);
        _cancellationRegistrations.Add(linkedCancellationTokenSource.Token.Register(CancelWorkflow));
    }

    public long Id { get; set; }

    public long? ParentWorkflowInstanceId { get; set; }

    public string? TriggerActivityId { get; set; }

    public string CorrelationId { get; set; }

    public IDictionary<string, object> Properties { get; set; } = null!;

    public IDictionary<string, object> Input { get; set; } = null!;

    public IDictionary<string, object> Output { get; set; } = new Dictionary<string, object>();

    public WorkflowStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public ICollection<Bookmark> OriginalBookmarks { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public Diff<Bookmark> BookmarksDiff => Diff.For(OriginalBookmarks, Bookmarks);

    public ExecuteActivityDelegate? ExecuteDelegate { get; set; }

    public ExpressionExecutionContext ExpressionExecutionContext { get; private set; } = null!;

    public MemoryRegister MemoryRegister { get; private set; } = null!;

    public WorkflowGraph WorkflowGraph { get; private set; } = null!;

    public CancellationToken CancellationToken { get; }

    public IReadOnlyCollection<ActivityExecutionContext> ActivityExecutionContexts
    {
        get => _activityExecutionContexts.AsReadOnly();
        internal set => _activityExecutionContexts = value.ToList();
    }

    public IActivityScheduler Scheduler { get; }

    public IActivityRegistry ActivityRegistry { get; }

    public IActivityRegistryLookupService ActivityRegistryLookup { get; }

    public IActivity Activity => WorkflowGraph.Workflow;

    public IEnumerable<Variable> Variables => WorkflowGraph.Workflow.Variables;

    public ICollection<ActivityCompletionCallbackEntry> CompletionCallbacks => new ReadOnlyCollection<ActivityCompletionCallbackEntry>(_completionCallbackEntries);

    public IServiceProvider ServiceProvider { get; } = null!;

    public T GetRequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    public object GetRequiredService(Type serviceType) => ServiceProvider.GetRequiredService(serviceType);

    internal void TransitionTo(WorkflowStatus status)
    {
        if (status.IsFinished())
            throw new($"Cannot transition from {Status} to {status}");

        Status = status;
        UpdatedAt = DateTime.Now;

        if (status.IsFinished())
            FinishedAt = UpdatedAt;

        if (status.IsFinished() || status == WorkflowStatus.Suspended)
        {
            foreach (var registration in _cancellationRegistrations)
                registration.Dispose();
        }
    }

    public ActivityWorkItem Schedule(ActivityNode activityNode, ActivityExecutionContext owner, ScheduleWorkOptions? options = null)
    {
        var schedulerStrategy = GetRequiredService<IWorkflowExecutionContextSchedulerStrategy>();
        return schedulerStrategy.Schedule(this, activityNode, owner, options);
    }

    #region CompletionCallback
    public void AddCompletionCallback(ActivityExecutionContext owner, ActivityNode child, ActivityCompletionCallback? completionCallback = null)
    {
        var entry = new ActivityCompletionCallbackEntry(owner, child, completionCallback);
        _completionCallbackEntries.Add(entry);
    }

    public ActivityCompletionCallbackEntry? PopCompletionCallback(ActivityExecutionContext owner, ActivityNode child)
    {
        var entry = _completionCallbackEntries.FirstOrDefault(x => x.Owner == owner && x.Child == child);

        if (entry == null)
            return null;

        RemoveCompletionCallback(entry);
        return entry;
    }

    public void RemoveCompletionCallback(ActivityCompletionCallbackEntry entry) => _completionCallbackEntries.Remove(entry);

    public void RemoveCompletionCallbacks(IEnumerable<ActivityCompletionCallbackEntry> entries)
    {
        foreach (var entry in entries.ToList())
            _completionCallbackEntries.Remove(entry);
    }

    public void ClearCompletionCallbacks()
    {
        _completionCallbackEntries.Clear();
    }
    #endregion

    #region Activity Method
    public IActivity? FindActivity(ActivityHandle handle)
    {
        return handle.ActivityId != null
            ? FindActivityById(handle.ActivityId)
            : handle.ActivityNodeId != null
                ? FindActivityByNodeId(handle.ActivityNodeId)
                : handle.ActivityInstanceId != null
                    ? FindActivityByInstanceId(handle.ActivityInstanceId ?? 0)
                        : null;
    }

    public ActivityNode? FindNodeById(string nodeId) => WorkflowGraph.NodeIdLookup.TryGetValue(nodeId, out var node) ? node : null;

    public ActivityNode? FindNodeByActivity(IActivity activity)
    {
        return WorkflowGraph.NodeActivityLookup.TryGetValue(activity, out var node) ? node : null;
    }

    public ActivityNode? FindNodeByActivityId(string activityId) => WorkflowGraph.Nodes.FirstOrDefault(x => x.Activity.Id == activityId);

    public IActivity? FindActivityByNodeId(string nodeId) => FindNodeById(nodeId)?.Activity;

    public IActivity? FindActivityById(string activityId) => FindNodeById(WorkflowGraph.NodeIdLookup.SingleOrDefault(n => n.Key.EndsWith(activityId)).Value.NodeId)?.Activity;

    public IActivity? FindActivityByInstanceId(long activityInstanceId) => _activityExecutionContexts.FirstOrDefault(x => x.Id == activityInstanceId)?.Activity;
    #endregion

    #region Property Method
    public T? GetProperty<T>(string key) => Properties.TryGetValue(key, out var value) ? value.ConvertTo<T>() : default;

    public void SetProperty<T>(string key, T value) => Properties[key] = value!;

    public T UpdateProperty<T>(string key, Func<T?, T> updater)
    {
        var value = GetProperty<T?>(key);
        value = updater(value);
        Properties[key] = value!;
        return value;
    }

    public bool HasProperty(string name) => Properties.ContainsKey(name);
    #endregion

    public void Cancel()
    {
        foreach (var source in _cancellationTokenSources)
            source.Cancel();

        _cancellationTokenSources.Clear();
    }

    private void CancelWorkflow()
    {
        Bookmarks.Clear();
        _completionCallbackEntries.Clear();

        if (Status.IsFinished())
            return;

        AddExecutionLogEntry("Workflow cancelled");

        TransitionTo(WorkflowStatus.Cancelled);

        foreach (var registration in _cancellationRegistrations)
            registration.Dispose();
    }

    public WorkflowExecutionLogEntry AddExecutionLogEntry(string eventName, string? message = null, object? payload = null, LogLevel logLevel = LogLevel.Information)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<WorkflowExecutionContext>>();
        //var payloadSerializer = ServiceProvider.GetRequiredService<IPayloadSerializer>();

        var serializedPayload = payload != null ? JsonSerializer.Serialize(payload) : null;

        var logEntry = new WorkflowExecutionLogEntry(
            Id,
            null,
            WorkflowGraph.Workflow.Id,
            WorkflowGraph.Workflow.Type,
            WorkflowGraph.Workflow.DefinitionInfo.Version,
            WorkflowGraph.Workflow.Name,
            WorkflowGraph.Workflow.DefinitionInfo.DefinitionId,
            Id,
            DateTime.Now,
            eventName,
            message,
            serializedPayload,
            logLevel.ToString());

        using var scope = logger.BeginScope(DictionaryConvert.Convert(logEntry));
        logger.Log(logLevel, $"Workflow Execution LOG:{eventName};message:{message}");

        return logEntry;
    }
}
