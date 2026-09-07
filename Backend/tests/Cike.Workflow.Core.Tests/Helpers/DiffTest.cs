using Cike.Workflow.Core.Helpers;

namespace Cike.Workflow.Core.Tests.Helpers;

[TestFixture]
public class DiffTest
{
    [Test]
    public void For_IdenticalCollections_ReturnsEmptyAddedAndRemoved()
    {
        var first = new List<int> { 1, 2, 3 };
        var second = new List<int> { 1, 2, 3 };

        var diff = Diff.For(first, second);

        Assert.That(diff.Added, Is.Empty);
        Assert.That(diff.Removed, Is.Empty);
        Assert.That(diff.Unchanged, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void For_CompletelyDifferentCollections_ReturnsCorrectSets()
    {
        var first = new List<int> { 1, 2, 3 };
        var second = new List<int> { 4, 5, 6 };

        var diff = Diff.For(first, second);

        Assert.That(diff.Added, Is.EquivalentTo(new[] { 4, 5, 6 }));
        Assert.That(diff.Removed, Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(diff.Unchanged, Is.Empty);
    }

    [Test]
    public void For_PartialOverlap_ReturnsCorrectAddedRemovedUnchanged()
    {
        var first = new List<int> { 1, 2, 3 };
        var second = new List<int> { 2, 3, 4 };

        var diff = Diff.For(first, second);

        Assert.That(diff.Added, Is.EquivalentTo(new[] { 4 }));
        Assert.That(diff.Removed, Is.EquivalentTo(new[] { 1 }));
        Assert.That(diff.Unchanged, Is.EquivalentTo(new[] { 2, 3 }));
    }

    [Test]
    public void For_EmptyFirstCollection_AllItemsAreAdded()
    {
        var first = new List<int>();
        var second = new List<int> { 1, 2 };

        var diff = Diff.For(first, second);

        Assert.That(diff.Added, Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(diff.Removed, Is.Empty);
        Assert.That(diff.Unchanged, Is.Empty);
    }

    [Test]
    public void For_EmptySecondCollection_AllItemsAreRemoved()
    {
        var first = new List<int> { 1, 2 };
        var second = new List<int>();

        var diff = Diff.For(first, second);

        Assert.That(diff.Added, Is.Empty);
        Assert.That(diff.Removed, Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(diff.Unchanged, Is.Empty);
    }

    [Test]
    public void For_BothEmpty_ReturnsEmptyDiff()
    {
        var diff = Diff.For(new List<int>(), new List<int>());

        Assert.That(diff.Added, Is.Empty);
        Assert.That(diff.Removed, Is.Empty);
        Assert.That(diff.Unchanged, Is.Empty);
    }

    [Test]
    public void Empty_ReturnsEmptyDiff()
    {
        var diff = Diff.Empty<string>();

        Assert.That(diff.Added, Is.Empty);
        Assert.That(diff.Removed, Is.Empty);
        Assert.That(diff.Unchanged, Is.Empty);
    }

    [Test]
    public void From_ConstructsDiffWithExplicitSets()
    {
        var added = new List<string> { "a" };
        var removed = new List<string> { "b" };
        var unchanged = new List<string> { "c" };

        var diff = Diff.From(added, removed, unchanged);

        Assert.That(diff.Added, Is.SameAs(added));
        Assert.That(diff.Removed, Is.SameAs(removed));
        Assert.That(diff.Unchanged, Is.SameAs(unchanged));
    }

    [Test]
    public void For_WithCustomComparer_UsesComparer()
    {
        var first = new List<string> { "A", "B" };
        var second = new List<string> { "a", "c" };

        var diff = Diff.For(first, second, StringComparer.OrdinalIgnoreCase);

        Assert.That(diff.Added, Is.EquivalentTo(new[] { "c" }));
        Assert.That(diff.Removed, Is.EquivalentTo(new[] { "B" }));
        Assert.That(diff.Unchanged, Is.EquivalentTo(new[] { "A" }));
    }
}
