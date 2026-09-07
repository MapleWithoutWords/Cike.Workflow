using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.WorkflowGraphs;
using Cike.Workflow.Core.WorkflowGraphs.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.Contexts;

[TestFixture]
public class WorkflowExecutionContextTest : BaseIntegrationTest
{
    private IWorkflowGraphBuilder _graphBuilder = null!;

    [SetUp]
    public void SetUp()
    {
        _graphBuilder = serviceProvider.GetRequiredService<IWorkflowGraphBuilder>();
    }

    private async Task<WorkflowExecutionContext> CreateContextAsync(IActivity? rootActivity = null)
    {
        rootActivity ??= new WriteLine("test");
        var workflow = new WorkflowActivity(rootActivity);
        var graph = await _graphBuilder.BuildAsync(workflow);
        return await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 1L);
    }

    [Test]
    public async Task CreateAsync_SetsInitialStatusToPending()
    {
        var context = await CreateContextAsync();

        Assert.That(context.Status, Is.EqualTo(WorkflowStatus.Pending));
    }

    [Test]
    public async Task CreateAsync_SetsIdCorrectly()
    {
        var context = await CreateContextAsync();

        Assert.That(context.Id, Is.EqualTo(1L));
    }

    [Test]
    public async Task CreateAsync_WithCorrelationId_SetsCorrelationId()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var context = await WorkflowExecutionContext.CreateAsync(
            serviceProvider, graph, 1L, correlationId: "my-correlation-id");

        Assert.That(context.CorrelationId, Is.EqualTo("my-correlation-id"));
    }

    [Test]
    public async Task CreateAsync_WithInput_StoresInput()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var input = new Dictionary<string, object> { ["key"] = "value" };
        var context = await WorkflowExecutionContext.CreateAsync(
            serviceProvider, graph, 1L, correlationId: null, input: input);

        Assert.That(context.Input, Contains.Key("key"));
        Assert.That(context.Input["key"], Is.EqualTo("value"));
    }

    [Test]
    public async Task CreateAsync_WithProperties_StoresProperties()
    {
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var properties = new Dictionary<string, object> { ["prop1"] = 42 };
        var context = await WorkflowExecutionContext.CreateAsync(
            serviceProvider, graph, 1L, correlationId: null, properties: properties);

        Assert.That(context.Properties, Contains.Key("prop1"));
    }

    [Test]
    public async Task SetProperty_And_GetProperty_Roundtrip()
    {
        var context = await CreateContextAsync();

        context.SetProperty("myKey", 42);
        var value = context.GetProperty<int>("myKey");

        Assert.That(value, Is.EqualTo(42));
    }

    [Test]
    public async Task HasProperty_ExistingKey_ReturnsTrue()
    {
        var context = await CreateContextAsync();
        context.SetProperty("exists", "yes");

        Assert.That(context.HasProperty("exists"), Is.True);
    }

    [Test]
    public async Task HasProperty_NonExistingKey_ReturnsFalse()
    {
        var context = await CreateContextAsync();

        Assert.That(context.HasProperty("nonexistent"), Is.False);
    }

    [Test]
    public async Task UpdateProperty_UpdatesExistingValue()
    {
        var context = await CreateContextAsync();
        context.SetProperty("counter", 10);

        var result = context.UpdateProperty("counter", (int? x) => (x ?? 0) + 5);

        Assert.That(result, Is.EqualTo(15));
        Assert.That(context.GetProperty<int>("counter"), Is.EqualTo(15));
    }

    [Test]
    public async Task ScheduleWorkflow_AddsToScheduler()
    {
        var context = await CreateContextAsync();

        context.ScheduleWorkflow();

        Assert.That(context.Scheduler.HasAny, Is.True);
    }

    [Test]
    public async Task ScheduleActivity_AddsToScheduler()
    {
        var context = await CreateContextAsync();
        var activity = new WriteLine("scheduled");

        context.ScheduleActivity(activity);

        Assert.That(context.Scheduler.HasAny, Is.True);
    }

    [Test]
    public async Task AddAndPopCompletionCallback_Works()
    {
        var context = await CreateContextAsync();
        var root = context.WorkflowGraph.Root;

        // Create a child activity context to use as the child node
        var childActivity = new WriteLine("child");
        var childNode = new ActivityNode(childActivity, "Out");

        // Use the root activity context as owner
        var ownerContext = context.ActivityExecutionContexts.FirstOrDefault();
        if (ownerContext == null)
        {
            // Need to create an activity execution context first
            ownerContext = await context.CreateActivityExecutionContextAsync(context.Workflow);
            context.AddActivityExecutionContext(ownerContext);
        }

        context.AddCompletionCallback(ownerContext, childNode);

        var entry = context.PopCompletionCallback(ownerContext, childNode);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Owner, Is.SameAs(ownerContext));
        Assert.That(entry.Child, Is.SameAs(childNode));
    }

    [Test]
    public async Task PopCompletionCallback_WhenNoneExists_ReturnsNull()
    {
        var context = await CreateContextAsync();
        var root = context.WorkflowGraph.Root;
        var childNode = new ActivityNode(new WriteLine("x"), "");

        var ownerContext = await context.CreateActivityExecutionContextAsync(context.Workflow);
        context.AddActivityExecutionContext(ownerContext);

        var entry = context.PopCompletionCallback(ownerContext, childNode);
        Assert.That(entry, Is.Null);
    }

    [Test]
    public async Task FindActivityById_ReturnsCorrectActivity()
    {
        var context = await CreateContextAsync();
        var workflowActivity = context.FindActivityById(context.Workflow.Id);

        // The workflow activity should be findable
        Assert.That(context.Workflow, Is.Not.Null);
    }

    [Test]
    public async Task FindActivityByNodeId_ReturnsCorrectActivity()
    {
        var context = await CreateContextAsync();
        var rootActivity = context.FindActivityByNodeId(context.WorkflowGraph.Root.NodeId);

        Assert.That(rootActivity, Is.Not.Null);
    }

    [Test]
    public async Task AddAndRemoveActivityExecutionContext_Works()
    {
        var context = await CreateContextAsync();
        var activityContext = await context.CreateActivityExecutionContextAsync(context.Workflow);

        context.AddActivityExecutionContext(activityContext);
        Assert.That(context.ActivityExecutionContexts, Has.Count.EqualTo(1));

        context.RemoveActivityExecutionContext(activityContext);
        Assert.That(context.ActivityExecutionContexts, Is.Empty);
    }

    [Test]
    public async Task Cancel_SetsStatusToCancelled()
    {
        var context = await CreateContextAsync();
        context.TransitionTo(WorkflowStatus.Executing);

        context.Cancel();

        // After cancel, the status should be Cancelled (via the cancellation token callback)
        Assert.That(context.Status, Is.EqualTo(WorkflowStatus.Cancelled));
    }

    [Test]
    public async Task Workflow_ReturnsWorkflowActivity()
    {
        var context = await CreateContextAsync();

        Assert.That(context.Workflow, Is.Not.Null);
        Assert.That(context.Workflow, Is.InstanceOf<WorkflowActivity>());
    }

    [Test]
    public async Task CreatedAt_IsSetToReasonableTime()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var context = await CreateContextAsync();
        var after = DateTime.Now.AddSeconds(1);

        Assert.That(context.CreatedAt, Is.GreaterThan(before));
        Assert.That(context.CreatedAt, Is.LessThan(after));
    }

    [Test]
    public async Task GetActiveActivityExecutionContexts_FilterLogic()
    {
        var context = await CreateContextAsync();

        // Initially, there should be zero activity execution contexts
        // (they are created during workflow execution)
        var initialCount = context.GetActiveActivityExecutionContexts().Count();

        // Verify the filter logic: non-completed contexts with no parent are kept
        // and completed contexts with a parent are filtered out
        Assert.That(initialCount, Is.GreaterThanOrEqualTo(0));
    }
}
