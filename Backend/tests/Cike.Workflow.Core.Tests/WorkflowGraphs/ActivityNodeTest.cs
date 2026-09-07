using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.WorkflowGraphs.Models;

namespace Cike.Workflow.Core.Tests.WorkflowGraphs;

[TestFixture]
public class ActivityNodeTest
{
    private static IActivity CreateStubActivity(string id)
    {
        return new WriteLine(id) { Id = id, NodeId = id, Code = id };
    }

    [Test]
    public void Constructor_SetsActivityAndPort()
    {
        var activity = CreateStubActivity("a1");
        var node = new ActivityNode(activity, "Out");

        Assert.That(node.Activity, Is.SameAs(activity));
        Assert.That(node.Port, Is.EqualTo("Out"));
    }

    [Test]
    public void NodeId_WithNoParents_ReturnsActivityId()
    {
        var activity = CreateStubActivity("root");
        var node = new ActivityNode(activity, "");

        Assert.That(node.NodeId, Is.EqualTo("root"));
    }

    [Test]
    public void NodeId_WithParent_IncludesParentId()
    {
        var parentActivity = CreateStubActivity("parent");
        var childActivity = CreateStubActivity("child");
        var parentNode = new ActivityNode(parentActivity, "");
        var childNode = new ActivityNode(childActivity, "");

        parentNode.AddChild(childNode);
        childNode.AddParent(parentNode);

        Assert.That(childNode.NodeId, Does.Contain("parent"));
        Assert.That(childNode.NodeId, Does.Contain("child"));
    }

    [Test]
    public void AddParent_AddsToParentsCollection()
    {
        var parent = new ActivityNode(CreateStubActivity("p"), "");
        var child = new ActivityNode(CreateStubActivity("c"), "");

        child.AddParent(parent);

        Assert.That(child.Parents, Has.Count.EqualTo(1));
        Assert.That(child.Parents.First(), Is.SameAs(parent));
    }

    [Test]
    public void AddChild_AddsToChildrenCollection()
    {
        var parent = new ActivityNode(CreateStubActivity("p"), "");
        var child = new ActivityNode(CreateStubActivity("c"), "");

        parent.AddChild(child);

        Assert.That(parent.Children, Has.Count.EqualTo(1));
        Assert.That(parent.Children.First(), Is.SameAs(child));
    }

    [Test]
    public void Descendants_ReturnsAllDescendants()
    {
        var root = new ActivityNode(CreateStubActivity("root"), "");
        var child1 = new ActivityNode(CreateStubActivity("c1"), "");
        var child2 = new ActivityNode(CreateStubActivity("c2"), "");
        var grandchild = new ActivityNode(CreateStubActivity("gc1"), "");

        root.AddChild(child1);
        root.AddChild(child2);
        child1.AddChild(grandchild);

        var descendants = root.Descendants().ToList();

        Assert.That(descendants, Has.Count.EqualTo(3));
        Assert.That(descendants, Does.Contain(child1));
        Assert.That(descendants, Does.Contain(child2));
        Assert.That(descendants, Does.Contain(grandchild));
    }

    [Test]
    public void Ancestors_ReturnsAllAncestors()
    {
        var root = new ActivityNode(CreateStubActivity("root"), "");
        var child = new ActivityNode(CreateStubActivity("child"), "");
        var grandchild = new ActivityNode(CreateStubActivity("gc"), "");

        root.AddChild(child);
        child.AddParent(root);
        child.AddChild(grandchild);
        grandchild.AddParent(child);

        var ancestors = grandchild.Ancestors().ToList();

        Assert.That(ancestors, Has.Count.EqualTo(2));
        Assert.That(ancestors, Does.Contain(child));
        Assert.That(ancestors, Does.Contain(root));
    }

    [Test]
    public void Siblings_ReturnsParentChildren()
    {
        var parent = new ActivityNode(CreateStubActivity("p"), "");
        var child1 = new ActivityNode(CreateStubActivity("c1"), "");
        var child2 = new ActivityNode(CreateStubActivity("c2"), "");

        parent.AddChild(child1);
        parent.AddChild(child2);
        child1.AddParent(parent);
        child2.AddParent(parent);

        var siblings = child1.Siblings().ToList();

        Assert.That(siblings, Has.Count.EqualTo(2));
        Assert.That(siblings, Does.Contain(child1));
        Assert.That(siblings, Does.Contain(child2));
    }

    [Test]
    public void Flatten_ReturnsSelfAndAllDescendants()
    {
        var root = new ActivityNode(CreateStubActivity("root"), "");
        var child = new ActivityNode(CreateStubActivity("c"), "");
        var grandchild = new ActivityNode(CreateStubActivity("gc"), "");

        root.AddChild(child);
        child.AddChild(grandchild);

        var flattened = root.Flatten().ToList();

        Assert.That(flattened, Has.Count.EqualTo(3));
        Assert.That(flattened[0], Is.SameAs(root));
        Assert.That(flattened[1], Is.SameAs(child));
        Assert.That(flattened[2], Is.SameAs(grandchild));
    }

    [Test]
    public void SiblingsAndCousins_ReturnsParentDescendants()
    {
        var parent = new ActivityNode(CreateStubActivity("p"), "");
        var child1 = new ActivityNode(CreateStubActivity("c1"), "");
        var child2 = new ActivityNode(CreateStubActivity("c2"), "");
        var grandchild = new ActivityNode(CreateStubActivity("gc"), "");

        parent.AddChild(child1);
        parent.AddChild(child2);
        child1.AddParent(parent);
        child2.AddParent(parent);
        child2.AddChild(grandchild);

        var siblingsAndCousins = child1.SiblingsAndCousins().ToList();

        Assert.That(siblingsAndCousins, Does.Contain(child2));
        Assert.That(siblingsAndCousins, Does.Contain(grandchild));
    }

    [Test]
    public void AddParent_InvalidatesNodeIdCache()
    {
        var root = new ActivityNode(CreateStubActivity("root"), "");
        var child = new ActivityNode(CreateStubActivity("child"), "");

        // First compute NodeId (caches it)
        var nodeIdBefore = child.NodeId;
        Assert.That(nodeIdBefore, Is.EqualTo("child"));

        // Add parent - should invalidate cache
        root.AddChild(child);
        child.AddParent(root);

        var nodeIdAfter = child.NodeId;
        Assert.That(nodeIdAfter, Does.Contain("root"));
    }
}
