using Cike.Workflow.Core.Enums;

namespace Cike.Workflow.Core.Tests.Enums;

[TestFixture]
public class ActivityStatusTest
{
    [TestCase(ActivityStatus.Pending, true)]
    [TestCase(ActivityStatus.Running, true)]
    [TestCase(ActivityStatus.Faulted, true)]
    [TestCase(ActivityStatus.Completed, false)]
    [TestCase(ActivityStatus.Canceled, false)]
    public void CanCancelActivity_ReturnsExpectedResult(ActivityStatus status, bool expected)
    {
        Assert.That(status.CanCancelActivity(), Is.EqualTo(expected));
    }
}
