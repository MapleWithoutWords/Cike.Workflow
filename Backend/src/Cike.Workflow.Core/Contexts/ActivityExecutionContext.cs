using Cike.Core.Extensions.System;
using Cike.Core.Hashers;
using Cike.Workflow.Core.Contexts.Models;
using Cike.Workflow.Core.Helpers;
using Cike.Workflow.Core.Schedulers;
using Cike.Workflow.Core.Schedulers.Models;
using Cike.Workflow.Core.Variables;
using System.Text.Json;

namespace Cike.Workflow.Core.Contexts;

public class ActivityExecutionContext : IExecutionContext
{
    private ActivityExecutionContext? _parentActivityExecutionContext;
    private List<Bookmark> _newBookmarks = [];

    public ActivityExecutionContext(
        long id,
        WorkflowExecutionContext workflowExecutionContext,
        ActivityExecutionContext? parentActivityExecutionContext,
        IActivity activity,
        ActivityDescriptor activityDescriptor,
        DateTime startedAt,
        CancellationToken cancellationToken)
    {
        Properties = new ChangeTrackingDictionary<string, object>(Taint);
        ActivityState = new ChangeTrackingDictionary<string, object>(Taint);
        ActivityInput = new ChangeTrackingDictionary<string, object>(Taint);
        WorkflowExecutionContext = workflowExecutionContext;
        ParentActivityExecutionContext = parentActivityExecutionContext;
        var expressionExecutionContextProps = ExpressionExecutionContextHelper.CreateActivityExecutionContextPropertiesFrom(workflowExecutionContext, workflowExecutionContext.Input);
        expressionExecutionContextProps[ExpressionExecutionContextHelper.ActivityKey] = activity;
        ExpressionExecutionContext = new(workflowExecutionContext.ServiceProvider, new(), parentActivityExecutionContext?.ExpressionExecutionContext ?? workflowExecutionContext.ExpressionExecutionContext, expressionExecutionContextProps, Taint, cancellationToken);
        Activity = activity;
        ActivityDescriptor = activityDescriptor;
        CreatedAt = startedAt;
        Status = ActivityStatus.Pending;
        CancellationToken = cancellationToken;
        Id = id;
    }

    public long Id { get; set; }

    public ActivityStatus Status { get; set; }

    public IActivity Activity { get; set; } = null!;

    public IEnumerable<Variable> Variables
    {
        get
        {
            var containerVariables = (Activity as IVariableContainer)?.Variables ?? Enumerable.Empty<Variable>();
            var dynamicVariables = DynamicVariables;
            var mergedVariables = new Dictionary<string, Variable>();

            foreach (var containerVariable in containerVariables)
            {
                var name = !string.IsNullOrEmpty(containerVariable.Name) ? containerVariable.Name : containerVariable.Id;
                mergedVariables[name] = containerVariable;
            }

            foreach (var dynamicVariable in dynamicVariables)
            {
                var name = !string.IsNullOrEmpty(dynamicVariable.Name) ? dynamicVariable.Name : dynamicVariable.Id;
                mergedVariables[name] = dynamicVariable;
            }
            return mergedVariables.Values;
        }
    }

    public ICollection<Variable> DynamicVariables { get; set; } = new List<Variable>();

    public IDictionary<string, object> Properties { get; private set; }

    public IDictionary<string, object> ActivityState { get; }

    public IDictionary<string, object> ActivityInput { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public bool IsDirty { get; private set; }

    public IDictionary<object, object> TransientProperties { get; private set; } = new Dictionary<object, object>();

    public ExpressionExecutionContext ExpressionExecutionContext { get; } = null!;

    public ActivityDescriptor ActivityDescriptor { get; } = null!;

    public CancellationToken CancellationToken { get; }

    public WorkflowExecutionContext WorkflowExecutionContext { get; } = null!;

    public ActivityExecutionContext? ParentActivityExecutionContext
    {
        get => _parentActivityExecutionContext;
        internal set
        {
            _parentActivityExecutionContext = value;
            _parentActivityExecutionContext?.Children.Add(this);
        }
    }

    public ISet<ActivityExecutionContext> Children { get; } = new HashSet<ActivityExecutionContext>();

    public IEnumerable<Bookmark> NewBookmarks => _newBookmarks.AsReadOnly();

    public ActivityNode ActivityNode => WorkflowExecutionContext.FindNodeByActivity(Activity)!;

    #region Properties
    public T? GetProperty<T>(string key) => Properties.TryGetValue<T?>(key, out var value) ? value : default;

    public T GetProperty<T>(string key, Func<T> defaultValue)
    {
        if (Properties.TryGetValue<T?>(key, out var value))
            return value!;

        value = defaultValue();
        Properties[key] = value!;

        return value!;
    }

    public void SetProperty<T>(string key, T? value) => Properties[key] = value!;

    public T UpdateProperty<T>(string key, Func<T?, T> updater) where T : notnull
    {
        var value = GetProperty<T?>(key);
        value = updater(value);
        Properties[key] = value;
        return value;
    }

    public void RemoveProperty(string key) => Properties.Remove(key);
    #endregion

    #region Parent/Children
    public ActivityExecutionContext? FindParentWithVariableContainer()
    {
        return FindParent(x => x.Activity is IVariableContainer);
    }

    public ActivityExecutionContext? FindParent(Func<ActivityExecutionContext, bool> predicate)
    {
        var currentContext = this;

        while (currentContext != null)
        {
            if (predicate(currentContext))
                return currentContext;

            currentContext = currentContext.ParentActivityExecutionContext;
        }

        return null;
    }

    public IEnumerable<ActivityExecutionContext> GetAncestors()
    {
        var current = ParentActivityExecutionContext;

        while (current != null)
        {
            yield return current;
            current = current.ParentActivityExecutionContext;
        }
    }

    public IEnumerable<ActivityExecutionContext> GetChildren()
    {
        return Children;
    }

    public IEnumerable<ActivityExecutionContext> GetDescendants()
    {
        var children = Children.ToList();

        foreach (var child in children)
        {
            yield return child;

            foreach (var descendant in child.GetDescendants())
                yield return descendant;
        }
    }
    #endregion

    public void Taint()
    {
        if (!IsDirty)
            IsDirty = true;
    }

    public void ClearTaint()
    {
        if (IsDirty)
            IsDirty = false;
    }

    #region Scheduler Activity
    public ValueTask ScheduleActivityAsync(IActivity? activity, ActivityCompletionCallback? completionCallback, IEnumerable<Variable>? variables = null)
    {
        var options = new ScheduleWorkOptions
        {
            CompletionCallback = completionCallback,
            Variables = variables?.ToList()
        };
        return ScheduleActivityAsync(activity, options);
    }

    public async ValueTask ScheduleActivityAsync(IActivity? activity, ScheduleWorkOptions? options = null)
    {
        await ScheduleActivityAsync(activity, this, options);
    }

    public async ValueTask ScheduleActivityAsync(IActivity? activity, ActivityExecutionContext? owner, ScheduleWorkOptions? options = null)
    {
        var schedulerStrategy = GetRequiredService<IActivityExecutionContextSchedulerStrategy>();
        await schedulerStrategy.ScheduleActivityAsync(this, activity, owner, options);
    }

    public async Task ScheduleActivityAsync(ActivityNode? activityNode, ActivityExecutionContext? owner = null, ScheduleWorkOptions? options = null)
    {
        var schedulerStrategy = GetRequiredService<IActivityExecutionContextSchedulerStrategy>();
        await schedulerStrategy.ScheduleActivityAsync(this, activityNode, owner, options);
    }

    public async ValueTask ScheduleActivitiesAsync(params IActivity?[] activities) => await ScheduleActivities(activities);

    public ValueTask ScheduleActivities(IEnumerable<IActivity?> activities, ActivityCompletionCallback? completionCallback, IEnumerable<Variable>? variables = null)
    {
        var options = new ScheduleWorkOptions
        {
            CompletionCallback = completionCallback,
            Variables = variables?.ToList()
        };
        return ScheduleActivities(activities, options);
    }

    public async ValueTask ScheduleActivities(IEnumerable<IActivity?> activities, ScheduleWorkOptions? options = null)
    {
        foreach (var activity in activities)
            await ScheduleActivityAsync(activity, options);
    }
    #endregion

    #region Bookmarks
    public void CreateBookmarks(IEnumerable<object> payloads, ExecuteActivityDelegate? callback = null, bool includeActivityInstanceId = true, string? bookmarkName = null)
    {
        foreach (var payload in payloads)
            CreateBookmark(new()
            {
                Stimulus = payload,
                Callback = callback,
                BookmarkName = bookmarkName,
                IncludeActivityInstanceId = includeActivityInstanceId
            });
    }

    public void AddBookmarks(IEnumerable<Bookmark> bookmarks)
    {
        WorkflowExecutionContext.Bookmarks.AddRange(bookmarks);
        Taint();
    }

    public void AddBookmark(Bookmark bookmark)
    {
        _newBookmarks.Add(bookmark);
        WorkflowExecutionContext.Bookmarks.Add(bookmark);
        Taint();
    }

    public Bookmark CreateBookmark(ExecuteActivityDelegate callback, IDictionary<string, string>? metadata = null)
    {
        return CreateBookmark(new()
        {
            Callback = callback,
            Metadata = metadata
        });
    }

    public Bookmark CreateBookmark(object stimulus, ExecuteActivityDelegate? callback, bool includeActivityInstanceId = true, IDictionary<string, string>? customProperties = null)
    {
        return CreateBookmark(new()
        {
            Stimulus = stimulus,
            Callback = callback,
            IncludeActivityInstanceId = includeActivityInstanceId,
            Metadata = customProperties
        });
    }

    public Bookmark CreateBookmark(object stimulus, bool includeActivityInstanceId, IDictionary<string, string>? customProperties = null)
    {
        return CreateBookmark(new()
        {
            Stimulus = stimulus,
            IncludeActivityInstanceId = includeActivityInstanceId,
            Metadata = customProperties
        });
    }

    public Bookmark CreateBookmark(object stimulus, IDictionary<string, string>? metadata = null)
    {
        return CreateBookmark(new()
        {
            Stimulus = stimulus,
            Metadata = metadata
        });
    }

    public Bookmark CreateBookmark(CreateBookmarkArgs? options = null)
    {
        var payload = options?.Stimulus;
        var callback = options?.Callback;
        var bookmarkName = options?.BookmarkName ?? Activity.Type;
        var bookmarkHasher = GetRequiredService<IHasher>();
        var identityGenerator = GetRequiredService<ISnowflakeIdGenerator>();
        var includeActivityInstanceId = options?.IncludeActivityInstanceId ?? true;
        var hash = bookmarkHasher.Hash(new object?[] { bookmarkName, payload, includeActivityInstanceId ? Id : null });
        var bookmarkId = options?.BookmarkId ?? identityGenerator.NextId();

        var bookmark = new Bookmark(
            bookmarkId,
            bookmarkName,
            hash,
            payload,
            Activity.Id,
            ActivityNode.NodeId,
            Id,
            DateTime.Now,
            options?.AutoBurn ?? true,
            callback?.Method.Name,
            options?.AutoComplete ?? true,
            options?.Metadata);

        AddBookmark(bookmark);
        return bookmark;
    }

    public void ClearBookmarks()
    {
        _newBookmarks.Clear();
        WorkflowExecutionContext.Bookmarks.RemoveAll(x => x.ActivityInstanceId == Id);
        Taint();
    }
    #endregion

    public T GetRequiredService<T>() where T : notnull => WorkflowExecutionContext.GetRequiredService<T>();

    public object GetRequiredService(Type serviceType) => WorkflowExecutionContext.GetRequiredService(serviceType);

    public async ValueTask SendSignalAsync(object signal)
    {
        var receivingContexts = new[]
        {
                this
            }.Concat(this.GetAncestors()).ToList();
        var logger = this.GetRequiredService<ILogger<ActivityExecutionContext>>();
        var signalType = signal.GetType();
        var signalTypeName = signalType.Name;

        // Let all ancestors receive the signal.
        foreach (var ancestorContext in receivingContexts)
        {
            var signalContext = new SignalContext(ancestorContext, this, WorkflowExecutionContext.CancellationToken);

            if (ancestorContext.Activity is not ISignalHandler handler)
                continue;

            logger.LogDebug("Receiving signal {SignalType} on activity {ActivityId} of type {ActivityType}", signalTypeName, ancestorContext.Activity.Id, ancestorContext.Activity.Type);
            await handler.ReceiveSignalAsync(signal, signalContext);

            if (signalContext.StopPropagationRequested)
            {
                logger.LogDebug("Propagation of signal {SignalType} on activity {ActivityId} of type {ActivityType} was stopped", signalTypeName, ancestorContext.Activity.Id, ancestorContext.Activity.Type);
                return;
            }
        }
    }

    public async ValueTask CompleteActivityAsync(Outcomes? result = null)
    {
        // If the activity is not running, do nothing.
        if (Status != ActivityStatus.Running)
            return;

        // Cancel any non-completed child activities.
        var childContexts = Children.Where(x => x.Status.CanCancelActivity()).ToList();

        foreach (var childContext in childContexts)
            await childContext.CancelActivityAsync();

        // Mark the activity as complete.
        TransitionTo(ActivityStatus.Completed);

        // Add an execution log entry.
        AddExecutionLogEntry("Completed");

        // Send a signal.
        await this.SendSignalAsync(new ActivityCompletedSignal(result));

        // Clear bookmarks.
        ClearBookmarks();
        WorkflowExecutionContext.Bookmarks.RemoveAll(x => x.ActivityInstanceId == Id);

        // Remove completion callbacks.
        ClearCompletionCallbacks();

        // Remove all associated variables, unless this is the root context - in which case we want to keep the variables since we're not deleting that one.
        if (ParentActivityExecutionContext != null)
        {
            var variablePersistenceManager = GetRequiredService<IVariablePersistenceManager>();
            await variablePersistenceManager.DeleteVariablesAsync(this);
        }

        // Update the completed at timestamp.
        FinishedAt = DateTime.Now;
    }

    internal void WithdrawScheduledWork()
    {
        WorkflowExecutionContext.Scheduler.RemoveWhere(workItem => workItem.ExistingActivityExecutionContext == this || workItem.Owner == this);
    }

    private async Task CancelActivityAsync()
    {
        if (!Status.CanCancelActivity())
            return;

        WithdrawScheduledWork();
        TransitionTo(ActivityStatus.Canceled);
        ClearBookmarks();
        ClearCompletionCallbacks();
        WorkflowExecutionContext.Bookmarks.RemoveAll(x => x.ActivityNodeId == ActivityNode.NodeId);
        AddExecutionLogEntry("Canceled");
        await this.SendSignalAsync(new CancelSignal());
        await CancelChildActivitiesAsync();

        // ReSharper disable once MethodSupportsCancellation
        //await _publisher.SendAsync(new ActivityCancelled(this));
    }

    private async Task CancelChildActivitiesAsync()
    {
        var childContexts = WorkflowExecutionContext.ActivityExecutionContexts.Where(x => x.ParentActivityExecutionContext == this && x.Status.CanCancelActivity()).ToList();

        foreach (var childContext in childContexts)
            await childContext.CancelActivityAsync();
    }

    public void ClearCompletionCallbacks()
    {
        var entriesToRemove = WorkflowExecutionContext.CompletionCallbacks.Where(x => x.Owner == this).ToList();
        WorkflowExecutionContext.RemoveCompletionCallbacks(entriesToRemove);
    }

    public void TransitionTo(ActivityStatus status)
    {
        Status = status;
    }

    public WorkflowExecutionLogEntry AddExecutionLogEntry(string eventName, string? message = null, string? source = null, object? payload = null, LogLevel logLevel = LogLevel.Information)
    {
        var logger = WorkflowExecutionContext.GetRequiredService<ILogger<WorkflowExecutionContext>>();
        //var payloadSerializer = ServiceProvider.GetRequiredService<IPayloadSerializer>();

        var serializedPayload = payload != null ? JsonSerializer.Serialize(payload) : null;

        var logEntry = new WorkflowExecutionLogEntry(
            Id,
            ParentActivityExecutionContext?.Id ?? 0,
            Activity.Id,
            Activity.Type,
            Activity.Version,
            Activity.Name,
            Activity.NodeId,
            WorkflowExecutionContext.Id,
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
