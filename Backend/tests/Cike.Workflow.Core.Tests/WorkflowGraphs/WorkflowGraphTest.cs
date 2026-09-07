using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Models;
using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.Tests.WorkflowGraphs;

[TestFixture]
public class WorkflowGraphTest
{
    private static IActivity CreateStubActivity(string id)
    {
        return new WriteLine(id) { Id = id, NodeId = id, Code = id };
    }

    private WorkflowGraph CreateTestGraph()
    {
        var workflow = new WorkflowActivity();
        var rootActivity = CreateStubActivity("root");
        var childActivity = CreateStubActivity("child1");

        var rootNode = new ActivityNode(rootActivity, "");
        var childNode = new ActivityNode(childActivity, "Out");
        rootNode.AddChild(childNode);

        var nodes = new[] { rootNode, childNode };
        return new WorkflowGraph(workflow, rootNode, nodes);
    }

    [Test]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var graph = CreateTestGraph();

        Assert.That(graph.Root, Is.Not.Null);
        Assert.That(graph.Nodes, Has.Count.EqualTo(2));
        Assert.That(graph.NodeIdLookup, Has.Count.EqualTo(2));
        Assert.That(graph.NodeActivityLookup, Has.Count.EqualTo(2));
    }

    [Test]
    public void FindNodeById_ExistingId_ReturnsNode()
    {
        var graph = CreateTestGraph();
        var node = graph.FindNodeById("root");

        Assert.That(node, Is.Not.Null);
        Assert.That(node!.Activity.Id, Is.EqualTo("root"));
    }

    [Test]
    public void FindNodeById_NonExistingId_ReturnsNull()
    {
        var graph = CreateTestGraph();
        var node = graph.FindNodeById("nonexistent");

        Assert.That(node, Is.Null);
    }

    [Test]
    public void FindNodeByActivity_ExistingActivity_ReturnsNode()
    {
        var graph = CreateTestGraph();
        var activity = graph.Root.Activity;
        var node = graph.FindNodeByActivity(activity);

        Assert.That(node, Is.Not.Null);
        Assert.That(node!.Activity, Is.SameAs(activity));
    }

    [Test]
    public void FindNodeByActivity_NonExistingActivity_ReturnsNull()
    {
        var graph = CreateTestGraph();
        var unknownActivity = CreateStubActivity("unknown");
        var node = graph.FindNodeByActivity(unknownActivity);

        Assert.That(node, Is.Null);
    }

    [Test]
    public void FindNodeByActivityId_ExistingId_ReturnsNode()
    {
        var graph = CreateTestGraph();
        var node = graph.FindNodeByActivityId("child1");

        Assert.That(node, Is.Not.Null);
        Assert.That(node!.Activity.Id, Is.EqualTo("child1"));
    }

    [Test]
    public void FindActivityByNodeId_ExistingId_ReturnsActivity()
    {
        var graph = CreateTestGraph();
        var activity = graph.FindActivityByNodeId("root");

        Assert.That(activity, Is.Not.Null);
        Assert.That(activity!.Id, Is.EqualTo("root"));
    }

    [Test]
    public void FindActivityByNodeId_NonExistingId_ReturnsNull()
    {
        var graph = CreateTestGraph();
        var activity = graph.FindActivityByNodeId("nonexistent");

        Assert.That(activity, Is.Null);
    }

    [Test]
    public void FindActivity_WithActivityId_ReturnsCorrectActivity()
    {
        var graph = CreateTestGraph();
        var handle = ActivityHandle.FromActivityId("child1");
        var activity = graph.FindActivity(handle);

        Assert.That(activity, Is.Not.Null);
        Assert.That(activity!.Id, Is.EqualTo("child1"));
    }

    [Test]
    public void FindActivity_WithActivityNodeId_ReturnsCorrectActivity()
    {
        var graph = CreateTestGraph();
        var handle = ActivityHandle.FromActivityNodeId("root");
        var activity = graph.FindActivity(handle);

        Assert.That(activity, Is.Not.Null);
        Assert.That(activity!.Id, Is.EqualTo("root"));
    }

    [Test]
    public void FindActivity_WithNoMatch_ReturnsNull()
    {
        var graph = CreateTestGraph();
        var handle = new ActivityHandle(); // no identifiers set
        var activity = graph.FindActivity(handle);

        Assert.That(activity, Is.Null);
    }
}
