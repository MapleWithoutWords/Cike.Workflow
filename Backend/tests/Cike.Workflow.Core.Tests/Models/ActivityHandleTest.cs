using Cike.Workflow.Core.Models;

namespace Cike.Workflow.Core.Tests.Models;

[TestFixture]
public class ActivityHandleTest
{
    [Test]
    public void FromActivityId_SetsActivityId()
    {
        var handle = ActivityHandle.FromActivityId("activity-1");
        Assert.That(handle.ActivityId, Is.EqualTo("activity-1"));
        Assert.That(handle.ActivityNodeId, Is.Null);
        Assert.That(handle.ActivityInstanceId, Is.Null);
    }

    [Test]
    public void FromActivityNodeId_SetsActivityNodeId()
    {
        var handle = ActivityHandle.FromActivityNodeId("node-1");
        Assert.That(handle.ActivityNodeId, Is.EqualTo("node-1"));
        Assert.That(handle.ActivityId, Is.Null);
        Assert.That(handle.ActivityInstanceId, Is.Null);
    }

    [Test]
    public void FromActivityInstanceId_SetsActivityInstanceId()
    {
        var handle = ActivityHandle.FromActivityInstanceId(42L);
        Assert.That(handle.ActivityInstanceId, Is.EqualTo(42L));
        Assert.That(handle.ActivityId, Is.Null);
        Assert.That(handle.ActivityNodeId, Is.Null);
    }

    [Test]
    public void ToString_WithActivityId_ReturnsActivityId()
    {
        var handle = ActivityHandle.FromActivityId("test-id");
        Assert.That(handle.ToString(), Is.EqualTo("test-id"));
    }

    [Test]
    public void ToString_WithActivityNodeId_ReturnsNodeId()
    {
        var handle = ActivityHandle.FromActivityNodeId("node-id");
        Assert.That(handle.ToString(), Is.EqualTo("node-id"));
    }

    [Test]
    public void ToString_WithActivityInstanceId_ReturnsInstanceId()
    {
        var handle = ActivityHandle.FromActivityInstanceId(99L);
        Assert.That(handle.ToString(), Is.EqualTo("99"));
    }

    [Test]
    public void ToString_WithNothingSet_ReturnsEmptyString()
    {
        var handle = new ActivityHandle();
        Assert.That(handle.ToString(), Is.EqualTo(""));
    }
}
