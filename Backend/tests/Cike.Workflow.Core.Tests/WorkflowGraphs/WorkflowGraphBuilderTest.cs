using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.WorkflowGraphs;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.WorkflowGraphs;

[TestFixture]
public class WorkflowGraphBuilderTest : BaseIntegrationTest
{
    private IWorkflowGraphBuilder _graphBuilder = null!;

    [SetUp]
    public void SetUp()
    {
        _graphBuilder = serviceProvider.GetRequiredService<IWorkflowGraphBuilder>();
    }

    [Test]
    public async Task BuildAsync_SingleRootActivity_GeneratesHierarchicalNodeIds()
    {
        var workflow = new WorkflowActivity(new WriteLine("hello"));

        var graph = await _graphBuilder.BuildAsync(workflow);

        Assert.That(graph.Root.Activity.Id, Is.EqualTo("Workflow1"));
        Assert.That(graph.Root.NodeId, Is.EqualTo("Workflow1"));

        var writeLineNode = graph.FindNodeByActivityId("WriteLine1");
        Assert.That(writeLineNode, Is.Not.Null);
        Assert.That(writeLineNode!.NodeId, Is.EqualTo("Workflow1:WriteLine1"));
        Assert.That(writeLineNode.Activity.NodeId, Is.EqualTo("Workflow1:WriteLine1"));
    }

    [Test]
    public async Task BuildAsync_WithNestedSequence_GeneratesHierarchicalNodeIds()
    {
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities = [new WriteLine("a"), new WriteLine("b")]
        });

        var graph = await _graphBuilder.BuildAsync(workflow);

        var sequenceNode = graph.FindNodeByActivityId("Sequence1");
        Assert.That(sequenceNode, Is.Not.Null);
        Assert.That(sequenceNode!.NodeId, Is.EqualTo("Workflow1:Sequence1"));

        var firstWriteLine = graph.FindNodeByActivityId("WriteLine1");
        Assert.That(firstWriteLine, Is.Not.Null);
        Assert.That(firstWriteLine!.NodeId, Is.EqualTo("Workflow1:Sequence1:WriteLine1"));

        var secondWriteLine = graph.FindNodeByActivityId("WriteLine2");
        Assert.That(secondWriteLine, Is.Not.Null);
        Assert.That(secondWriteLine!.NodeId, Is.EqualTo("Workflow1:Sequence1:WriteLine2"));
    }

    [Test]
    public async Task BuildAsync_WithRepeatedActivityTypes_IncrementsGeneratedIdsInTraversalOrder()
    {
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities = [new Sequence(), new Sequence()]
        });

        var graph = await _graphBuilder.BuildAsync(workflow);

        var outerSequence = graph.FindNodeByActivityId("Sequence1");
        Assert.That(outerSequence, Is.Not.Null);
        Assert.That(outerSequence!.NodeId, Is.EqualTo("Workflow1:Sequence1"));

        var firstInnerSequence = graph.FindNodeByActivityId("Sequence2");
        Assert.That(firstInnerSequence, Is.Not.Null);
        Assert.That(firstInnerSequence!.NodeId, Is.EqualTo("Workflow1:Sequence1:Sequence2"));

        var secondInnerSequence = graph.FindNodeByActivityId("Sequence3");
        Assert.That(secondInnerSequence, Is.Not.Null);
        Assert.That(secondInnerSequence!.NodeId, Is.EqualTo("Workflow1:Sequence1:Sequence3"));
    }

    [Test]
    public async Task BuildAsync_WithPresetIds_PreservesIdsInNodeIds()
    {
        var writeLine = new WriteLine("preset") { Id = "my-activity" };
        var workflow = new WorkflowActivity(new Sequence { Activities = [writeLine] });

        var graph = await _graphBuilder.BuildAsync(workflow);

        Assert.That(writeLine.Id, Is.EqualTo("my-activity"));
        var writeLineNode = graph.FindNodeByActivityId("my-activity");
        Assert.That(writeLineNode, Is.Not.Null);
        Assert.That(writeLineNode!.NodeId, Is.EqualTo("Workflow1:Sequence1:my-activity"));
    }

    [Test]
    public async Task BuildAsync_AfterBuild_AllActivitiesNodeIdMatchesGraphNodeId()
    {
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities = [new WriteLine("a"), new Sequence { Activities = [new WriteLine("b")] }]
        });

        var graph = await _graphBuilder.BuildAsync(workflow);

        Assert.That(graph.Nodes, Is.Not.Empty);
        foreach (var node in graph.Nodes)
            Assert.That(node.Activity.NodeId, Is.EqualTo(node.NodeId), $"Activity '{node.Activity.Id}' NodeId mismatch");
    }

    [Test]
    public async Task BuildAsync_AfterBuild_FlattenedNodesAreReachableFromLookups()
    {
        var workflow = new WorkflowActivity(new Sequence
        {
            Activities = [new WriteLine("a")]
        });

        var graph = await _graphBuilder.BuildAsync(workflow);

        Assert.That(graph.Nodes, Has.Count.EqualTo(3));

        var byNodeId = graph.FindNodeById("Workflow1:Sequence1:WriteLine1");
        Assert.That(byNodeId, Is.Not.Null);
        Assert.That(byNodeId!.Activity.Id, Is.EqualTo("WriteLine1"));

        var handle = ActivityHandle.FromActivityNodeId("Workflow1:Sequence1:WriteLine1");
        Assert.That(graph.FindActivity(handle), Is.Not.Null);
    }
}
