using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.WorkflowGraphs;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.Runners;

[TestFixture]
public class WorkflowStateExtractorTest : BaseIntegrationTest
{
    private IWorkflowStateExtractor _extractor = null!;
    private IWorkflowGraphBuilder _graphBuilder = null!;

    [SetUp]
    public void SetUp()
    {
        _extractor = serviceProvider.GetRequiredService<IWorkflowStateExtractor>();
        _graphBuilder = serviceProvider.GetRequiredService<IWorkflowGraphBuilder>();
    }

    private async Task<WorkflowExecutionContext> CreateAndRunContextAsync(IActivity? rootActivity = null)
    {
        rootActivity ??= new WriteLine("test");
        var workflow = new WorkflowActivity(rootActivity);
        var graph = await _graphBuilder.BuildAsync(workflow);
        var context = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 1L, "corr-1");
        context.ScheduleWorkflow();
        // Transition to executing
        context.TransitionTo(WorkflowStatus.Executing);
        return context;
    }

    [Test]
    public async Task Extract_CapturesId()
    {
        var context = await CreateAndRunContextAsync();

        var state = _extractor.Extract(context);

        Assert.That(state.Id, Is.EqualTo(1L));
    }

    [Test]
    public async Task Extract_CapturesCorrelationId()
    {
        var context = await CreateAndRunContextAsync();

        var state = _extractor.Extract(context);

        Assert.That(state.CorrelationId, Is.EqualTo("corr-1"));
    }

    [Test]
    public async Task Extract_CapturesStatus()
    {
        var context = await CreateAndRunContextAsync();

        var state = _extractor.Extract(context);

        Assert.That(state.Status, Is.EqualTo(WorkflowStatus.Executing));
    }

    [Test]
    public async Task Extract_CapturesDefinitionInfo()
    {
        var context = await CreateAndRunContextAsync();

        var state = _extractor.Extract(context);

        Assert.That(state.DefinitionId, Is.Not.Null);
        Assert.That(state.DefinitionVersion, Is.GreaterThan(0));
    }

    [Test]
    public async Task Extract_CapturesCreatedAt()
    {
        var context = await CreateAndRunContextAsync();
        var createdAt = context.CreatedAt;

        var state = _extractor.Extract(context);

        Assert.That(state.CreatedAt, Is.EqualTo(createdAt));
    }

    [Test]
    public async Task Extract_CapturesProperties()
    {
        var context = await CreateAndRunContextAsync();
        context.SetProperty("key1", "value1");

        var state = _extractor.Extract(context);

        Assert.That(state.Properties, Contains.Key("key1"));
    }

    [Test]
    public async Task Extract_CapturesScheduledActivities()
    {
        var context = await CreateAndRunContextAsync();
        context.ScheduleActivity(new WriteLine("scheduled"));

        var state = _extractor.Extract(context);

        Assert.That(state.ScheduledActivities, Is.Not.Empty);
    }

    [Test]
    public async Task ApplyAsync_RestoresId()
    {
        var context = await CreateAndRunContextAsync();
        var state = _extractor.Extract(context);

        // Create a new context and apply state
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var newContext = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 999L);

        await _extractor.ApplyAsync(newContext, state);

        Assert.That(newContext.Id, Is.EqualTo(1L));
    }

    [Test]
    public async Task ApplyAsync_RestoresCorrelationId()
    {
        var context = await CreateAndRunContextAsync();
        var state = _extractor.Extract(context);

        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var newContext = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 999L);

        await _extractor.ApplyAsync(newContext, state);

        Assert.That(newContext.CorrelationId, Is.EqualTo("corr-1"));
    }

    [Test]
    public async Task ApplyAsync_RestoresProperties()
    {
        var context = await CreateAndRunContextAsync();
        context.SetProperty("restoreKey", 42);
        var state = _extractor.Extract(context);

        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var newContext = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 999L);

        await _extractor.ApplyAsync(newContext, state);

        Assert.That(newContext.Properties, Contains.Key("restoreKey"));
    }

    [Test]
    public async Task Extract_Apply_Roundtrip_PreservesCoreFields()
    {
        // Create and extract
        var context = await CreateAndRunContextAsync();
        context.SetProperty("roundtrip", "value");
        var state = _extractor.Extract(context);

        // Apply to new context
        var workflow = new WorkflowActivity(new WriteLine("test"));
        var graph = await _graphBuilder.BuildAsync(workflow);
        var restoredContext = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 999L);
        await _extractor.ApplyAsync(restoredContext, state);

        // Verify core fields survived the roundtrip
        Assert.That(restoredContext.Id, Is.EqualTo(context.Id));
        Assert.That(restoredContext.CorrelationId, Is.EqualTo(context.CorrelationId));
        Assert.That(restoredContext.CreatedAt, Is.EqualTo(context.CreatedAt));
    }

    [Test]
    public async Task Extract_WithEmptyContext_ProducesValidState()
    {
        var context = await CreateAndRunContextAsync();

        var state = _extractor.Extract(context);

        Assert.That(state, Is.Not.Null);
        Assert.That(state.ActivityExecutionContexts, Is.Not.Null);
        Assert.That(state.CompletionCallbacks, Is.Not.Null);
        Assert.That(state.ScheduledActivities, Is.Not.Null);
    }
}
