using System.Diagnostics;
using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Contexts;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Schedulers.Internals;
using Cike.Workflow.Core.Schedulers.Models;
using Cike.Workflow.Core.WorkflowGraphs;
using Cike.Workflow.Core.WorkflowGraphs.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.Performance;

/// <summary>
/// Performance tests for the workflow engine.
/// These tests measure execution time and throughput of key operations.
/// They use Assert.Warn for threshold violations rather than hard failures,
/// since CI environments may have variable performance.
/// </summary>
[TestFixture]
public class PerformanceTest : BaseIntegrationTest
{
    private IWorkflowGraphBuilder _graphBuilder = null!;
    private IWorkflowStateExtractor _extractor = null!;

    [SetUp]
    public void SetUp()
    {
        _graphBuilder = serviceProvider.GetRequiredService<IWorkflowGraphBuilder>();
        _extractor = serviceProvider.GetRequiredService<IWorkflowStateExtractor>();
    }

    #region 1. Simple Workflow E2E Execution Time

    [Test]
    public async Task E2E_SimpleWorkflow_ShouldCompleteWithinThreshold()
    {
        var workflow = new WorkflowActivity(new WriteLine("perf-test"));
        const int iterations = 50;
        var thresholds = new List<long>();

        // Warm-up
        await runner.RunAsync(workflow);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var result = await runner.RunAsync(workflow);
            Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / iterations;
        var totalMs = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"E2E Simple Workflow: {iterations} iterations in {totalMs}ms, avg {avgMs}ms/iteration");

        // Average per-iteration should be under 500ms (generous for CI)
        if (avgMs > 500)
            Assert.Warn($"Average execution time {avgMs}ms exceeds 500ms threshold");
    }

    [Test]
    public async Task E2E_SequenceWorkflow_ShouldCompleteWithinThreshold()
    {
        var sequence = new Sequence();
        for (var i = 0; i < 10; i++)
            sequence.Activities.Add(new WriteLine($"step-{i}"));

        var workflow = new WorkflowActivity(sequence);
        const int iterations = 20;

        // Warm-up
        await runner.RunAsync(workflow);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var result = await runner.RunAsync(workflow);
            Assert.That(result.WorkflowState.Status, Is.EqualTo(WorkflowStatus.Finished));
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / iterations;
        TestContext.WriteLine($"E2E Sequence(10) Workflow: {iterations} iterations in {sw.ElapsedMilliseconds}ms, avg {avgMs}ms/iteration");

        if (avgMs > 1000)
            Assert.Warn($"Average execution time {avgMs}ms exceeds 1000ms threshold");
    }

    #endregion

    #region 2. Large-Scale Activity Scheduling Throughput

    [Test]
    public void Scheduler_LargeScaleScheduleAndTake_MeasuresThroughput()
    {
        var scheduler = new QueueBasedActivityScheduler();
        const int count = 100_000;

        var sw = Stopwatch.StartNew();

        // Schedule
        for (var i = 0; i < count; i++)
        {
            var activity = new WriteLine($"item-{i}") { Id = $"item-{i}", NodeId = $"item-{i}", Code = $"item-{i}" };
            scheduler.Schedule(new ActivityWorkItem(activity));
        }
        var scheduleTime = sw.ElapsedMilliseconds;

        // Take
        sw.Restart();
        var taken = 0;
        while (scheduler.HasAny)
        {
            scheduler.Take();
            taken++;
        }
        var takeTime = sw.ElapsedMilliseconds;

        TestContext.WriteLine($"Scheduler throughput: Schedule {count} in {scheduleTime}ms, Take {taken} in {takeTime}ms");
        Assert.That(taken, Is.EqualTo(count));

        var totalOps = count * 2L; // schedule + take
        var opsPerSecond = totalOps * 1000.0 / Math.Max(1, scheduleTime + takeTime);
        TestContext.WriteLine($"Operations per second: {opsPerSecond:F0}");

        // Should handle at least 100k ops/sec
        if (opsPerSecond < 100_000)
            Assert.Warn($"Scheduler throughput {opsPerSecond:F0} ops/sec is below 100k threshold");
    }

    [Test]
    public void Scheduler_RemoveWhere_OnLargeQueue_MeasuresPerformance()
    {
        var scheduler = new QueueBasedActivityScheduler();
        const int count = 50_000;

        for (var i = 0; i < count; i++)
        {
            var activity = new WriteLine($"item-{i}") { Id = $"item-{i}", NodeId = $"item-{i}", Code = $"item-{i}" };
            scheduler.Schedule(new ActivityWorkItem(activity));
        }

        var sw = Stopwatch.StartNew();
        // Remove every other item
        var removed = scheduler.RemoveWhere(w => w.Activity.Id.EndsWith("0"));
        sw.Stop();

        TestContext.WriteLine($"RemoveWhere on {count} items: removed {removed} in {sw.ElapsedMilliseconds}ms");

        if (sw.ElapsedMilliseconds > 5000)
            Assert.Warn($"RemoveWhere took {sw.ElapsedMilliseconds}ms, exceeds 5s threshold");
    }

    #endregion

    #region 3. WorkflowStateExtractor Roundtrip Performance

    [Test]
    public async Task StateExtractor_ExtractApply_Roundtrip_MeasuresPerformance()
    {
        const int iterations = 100;
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities =
            {
                new WriteLine("step1"),
                new WriteLine("step2"),
                new WriteLine("step3")
            }
        });
        var graph = await _graphBuilder.BuildAsync(workflow);

        // Warm-up
        var warmupContext = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 1L);
        warmupContext.ScheduleWorkflow();
        warmupContext.TransitionTo(WorkflowStatus.Executing);
        _extractor.Extract(warmupContext);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var context = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, (long)(i + 100));
            context.ScheduleWorkflow();
            context.TransitionTo(WorkflowStatus.Executing);
            context.SetProperty("perfKey", i);

            var state = _extractor.Extract(context);

            var newContext = await WorkflowExecutionContext.CreateAsync(serviceProvider, graph, 999L);
            await _extractor.ApplyAsync(newContext, state);
        }
        sw.Stop();

        var avgMs = (double)sw.ElapsedMilliseconds / iterations;
        TestContext.WriteLine($"StateExtractor roundtrip: {iterations} iterations in {sw.ElapsedMilliseconds}ms, avg {avgMs:F1}ms/iteration");

        if (avgMs > 200)
            Assert.Warn($"StateExtractor roundtrip avg {avgMs:F1}ms exceeds 200ms threshold");
    }

    [Test]
    public async Task StateExtractor_Extract_WithActivityContexts_MeasuresScalability()
    {
        // Create a workflow with many activities to test scalability
        var sequence = new Sequence();
        for (var i = 0; i < 20; i++)
            sequence.Activities.Add(new WriteLine($"activity-{i}"));

        var workflow = new WorkflowActivity(sequence);
        var result = await runner.RunAsync(workflow);
        var context = result.WorkflowExecutionContext;

        const int iterations = 50;
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var state = _extractor.Extract(context);
            Assert.That(state, Is.Not.Null);
        }
        sw.Stop();

        var avgMs = (double)sw.ElapsedMilliseconds / iterations;
        TestContext.WriteLine($"StateExtractor extract with {context.ActivityExecutionContexts.Count} contexts: avg {avgMs:F1}ms over {iterations} iterations");

        if (avgMs > 100)
            Assert.Warn($"Extract avg {avgMs:F1}ms exceeds 100ms threshold");
    }

    #endregion

    #region 4. ActivityNode Graph Operations Performance

    [Test]
    public void ActivityNode_LargeTree_FlattenPerformance()
    {
        // Build a tree with ~1000 nodes
        var root = new ActivityNode(
            new WriteLine("root") { Id = "root", NodeId = "root", Code = "root" }, "");

        var nodeCount = 1;
        var currentLevel = new List<ActivityNode> { root };

        // Build 10 levels, each with branching factor ~2
        for (var level = 0; level < 10 && nodeCount < 1000; level++)
        {
            var nextLevel = new List<ActivityNode>();
            foreach (var parent in currentLevel)
            {
                for (var c = 0; c < 2 && nodeCount < 1000; c++)
                {
                    var child = new ActivityNode(
                        new WriteLine($"n{nodeCount}") { Id = $"n{nodeCount}", NodeId = $"n{nodeCount}", Code = $"n{nodeCount}" },
                        "");
                    parent.AddChild(child);
                    child.AddParent(parent);
                    nextLevel.Add(child);
                    nodeCount++;
                }
            }
            currentLevel = nextLevel;
        }

        TestContext.WriteLine($"Built tree with {nodeCount} nodes");

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var flattened = root.Flatten().ToList();
            Assert.That(flattened, Has.Count.EqualTo(nodeCount));
        }
        sw.Stop();

        var avgMs = (double)sw.ElapsedMilliseconds / 100;
        TestContext.WriteLine($"Flatten {nodeCount} nodes: avg {avgMs:F2}ms over 100 iterations");

        if (avgMs > 50)
            Assert.Warn($"Flatten avg {avgMs:F2}ms exceeds 50ms threshold");
    }

    [Test]
    public void ActivityNode_DeepChain_AncestorsPerformance()
    {
        // Build a deep chain of 500 nodes
        ActivityNode? previous = null;
        ActivityNode? deepest = null;
        const int depth = 500;

        for (var i = 0; i < depth; i++)
        {
            var node = new ActivityNode(
                new WriteLine($"n{i}") { Id = $"n{i}", NodeId = $"n{i}", Code = $"n{i}" }, "");

            if (previous != null)
            {
                previous.AddChild(node);
                node.AddParent(previous);
            }

            previous = node;
            deepest = node;
        }

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var ancestors = deepest!.Ancestors().ToList();
            Assert.That(ancestors, Has.Count.EqualTo(depth - 1));
        }
        sw.Stop();

        var avgMs = (double)sw.ElapsedMilliseconds / 100;
        TestContext.WriteLine($"Ancestors of depth-{depth} chain: avg {avgMs:F2}ms over 100 iterations");

        if (avgMs > 50)
            Assert.Warn($"Ancestors avg {avgMs:F2}ms exceeds 50ms threshold");
    }

    [Test]
    public void WorkflowGraph_LargeGraph_LookupPerformance()
    {
        // Build a workflow graph with many nodes (flat, no parent-child to keep NodeIds simple)
        const int nodeCount = 500;
        var nodes = new List<ActivityNode>();

        for (var i = 0; i < nodeCount; i++)
        {
            var activity = new WriteLine($"n{i}") { Id = $"n{i}", NodeId = $"n{i}", Code = $"n{i}" };
            var node = new ActivityNode(activity, "");
            nodes.Add(node);
        }

        var root = nodes[0];
        var workflow = new WorkflowActivity();
        var graph = new WorkflowGraph(workflow, root, nodes);

        // Test lookup performance
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 10_000; i++)
        {
            var nodeId = $"n{i % nodeCount}";
            var found = graph.FindNodeById(nodeId);
            Assert.That(found, Is.Not.Null);
        }
        sw.Stop();

        TestContext.WriteLine($"WorkflowGraph FindNodeById: 10000 lookups in {sw.ElapsedMilliseconds}ms");

        if (sw.ElapsedMilliseconds > 1000)
            Assert.Warn($"10000 lookups took {sw.ElapsedMilliseconds}ms, exceeds 1s threshold");
    }

    #endregion
}
