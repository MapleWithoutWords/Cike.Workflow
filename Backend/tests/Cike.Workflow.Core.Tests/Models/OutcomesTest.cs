using Cike.Workflow.Core.Models;

namespace Cike.Workflow.Core.Tests.Models;

[TestFixture]
public class OutcomesTest
{
    [Test]
    public void Default_HasDoneOutcome()
    {
        Assert.That(Outcomes.Default.Names, Is.EquivalentTo(new[] { null, "Done" }));
    }

    [Test]
    public void Empty_HasNoOutcomes()
    {
        Assert.That(Outcomes.Empty.Names, Is.Empty);
    }

    [Test]
    public void Constructor_WithMultipleNames_StoresAll()
    {
        var outcomes = new Outcomes("True", "False");
        Assert.That(outcomes.Names, Is.EquivalentTo(new[] { "True", "False" }));
    }

    [Test]
    public void Constructor_WithSingleName_StoresSingle()
    {
        var outcomes = new Outcomes("Done");
        Assert.That(outcomes.Names, Has.Length.EqualTo(1));
        Assert.That(outcomes.Names[0], Is.EqualTo("Done"));
    }
}
