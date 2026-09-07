using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Schedulers.Internals;
using Cike.Workflow.Core.Schedulers.Models;

namespace Cike.Workflow.Core.Tests.Schedulers;

[TestFixture]
public class QueueBasedActivitySchedulerTest
{
    private static IActivity CreateStubActivity(string id)
    {
        return new WriteLine(id) { Id = id, NodeId = id, Code = id };
    }

    private static ActivityWorkItem CreateWorkItem(string id)
    {
        return new ActivityWorkItem(CreateStubActivity(id));
    }

    [Test]
    public void HasAny_WhenEmpty_ReturnsFalse()
    {
        var scheduler = new QueueBasedActivityScheduler();
        Assert.That(scheduler.HasAny, Is.False);
    }

    [Test]
    public void HasAny_AfterSchedule_ReturnsTrue()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("a1"));
        Assert.That(scheduler.HasAny, Is.True);
    }

    [Test]
    public void Schedule_And_Take_ReturnsInFIFOOrder()
    {
        var scheduler = new QueueBasedActivityScheduler();
        var item1 = CreateWorkItem("a1");
        var item2 = CreateWorkItem("a2");

        scheduler.Schedule(item1);
        scheduler.Schedule(item2);

        Assert.That(scheduler.Take(), Is.SameAs(item1));
        Assert.That(scheduler.Take(), Is.SameAs(item2));
    }

    [Test]
    public void Take_WhenEmpty_ThrowsInvalidOperationException()
    {
        var scheduler = new QueueBasedActivityScheduler();
        Assert.Throws<InvalidOperationException>(() => scheduler.Take());
    }

    [Test]
    public void List_ReturnsAllScheduledItems()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("a1"));
        scheduler.Schedule(CreateWorkItem("a2"));

        var list = scheduler.List().ToList();
        Assert.That(list, Has.Count.EqualTo(2));
    }

    [Test]
    public void Any_WithMatchingPredicate_ReturnsTrue()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("target"));
        scheduler.Schedule(CreateWorkItem("other"));

        Assert.That(scheduler.Any(w => w.Activity.Id == "target"), Is.True);
    }

    [Test]
    public void Any_WithNonMatchingPredicate_ReturnsFalse()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("a1"));

        Assert.That(scheduler.Any(w => w.Activity.Id == "nonexistent"), Is.False);
    }

    [Test]
    public void Find_WithMatchingPredicate_ReturnsItem()
    {
        var scheduler = new QueueBasedActivityScheduler();
        var target = CreateWorkItem("target");
        scheduler.Schedule(target);
        scheduler.Schedule(CreateWorkItem("other"));

        var found = scheduler.Find(w => w.Activity.Id == "target");
        Assert.That(found, Is.SameAs(target));
    }

    [Test]
    public void Find_WithNonMatchingPredicate_ReturnsNull()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("a1"));

        var found = scheduler.Find(w => w.Activity.Id == "nonexistent");
        Assert.That(found, Is.Null);
    }

    [Test]
    public void RemoveWhere_RemovesMatchingItemsAndPreservesOrder()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("keep1"));
        scheduler.Schedule(CreateWorkItem("remove"));
        scheduler.Schedule(CreateWorkItem("keep2"));

        var removedCount = scheduler.RemoveWhere(w => w.Activity.Id == "remove");

        Assert.That(removedCount, Is.EqualTo(1));
        Assert.That(scheduler.HasAny, Is.True);

        var remaining = scheduler.List().ToList();
        Assert.That(remaining, Has.Count.EqualTo(2));
        Assert.That(remaining[0].Activity.Id, Is.EqualTo("keep1"));
        Assert.That(remaining[1].Activity.Id, Is.EqualTo("keep2"));
    }

    [Test]
    public void RemoveWhere_WithNoMatches_ReturnsZero()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("a1"));

        var removedCount = scheduler.RemoveWhere(w => w.Activity.Id == "nonexistent");

        Assert.That(removedCount, Is.EqualTo(0));
        Assert.That(scheduler.HasAny, Is.True);
    }

    [Test]
    public void RemoveWhere_RemovesMultipleItems()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("remove1"));
        scheduler.Schedule(CreateWorkItem("keep"));
        scheduler.Schedule(CreateWorkItem("remove2"));

        var removedCount = scheduler.RemoveWhere(w => w.Activity.Id.StartsWith("remove"));

        Assert.That(removedCount, Is.EqualTo(2));
        var remaining = scheduler.List().ToList();
        Assert.That(remaining, Has.Count.EqualTo(1));
        Assert.That(remaining[0].Activity.Id, Is.EqualTo("keep"));
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        var scheduler = new QueueBasedActivityScheduler();
        scheduler.Schedule(CreateWorkItem("a1"));
        scheduler.Schedule(CreateWorkItem("a2"));

        scheduler.Clear();

        Assert.That(scheduler.HasAny, Is.False);
        Assert.That(scheduler.List(), Is.Empty);
    }
}
