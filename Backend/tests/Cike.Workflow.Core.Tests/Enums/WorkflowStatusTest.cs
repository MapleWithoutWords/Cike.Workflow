using Cike.Workflow.Core.Enums;

namespace Cike.Workflow.Core.Tests.Enums;

[TestFixture]
public class WorkflowStatusTest
{
    [TestCase(WorkflowStatus.Pending, WorkflowMainStatus.Running)]
    [TestCase(WorkflowStatus.Executing, WorkflowMainStatus.Running)]
    [TestCase(WorkflowStatus.Suspended, WorkflowMainStatus.Running)]
    [TestCase(WorkflowStatus.Finished, WorkflowMainStatus.Finished)]
    [TestCase(WorkflowStatus.Cancelled, WorkflowMainStatus.Finished)]
    [TestCase(WorkflowStatus.Faulted, WorkflowMainStatus.Finished)]
    public void GetMainStatus_ReturnsExpectedMainStatus(WorkflowStatus status, WorkflowMainStatus expected)
    {
        Assert.That(status.GetMainStatus(), Is.EqualTo(expected));
    }

    [TestCase(WorkflowStatus.Finished, true)]
    [TestCase(WorkflowStatus.Cancelled, true)]
    [TestCase(WorkflowStatus.Faulted, true)]
    [TestCase(WorkflowStatus.Pending, false)]
    [TestCase(WorkflowStatus.Executing, false)]
    [TestCase(WorkflowStatus.Suspended, false)]
    public void IsFinished_ReturnsExpectedResult(WorkflowStatus status, bool expected)
    {
        Assert.That(status.IsFinished(), Is.EqualTo(expected));
    }

    [Test]
    public void Interrupted_ThrowsArgumentOutOfRange_OnGetMainStatus()
    {
        // WorkflowStatus.Interrupted is not mapped in the switch expression
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkflowStatus.Interrupted.GetMainStatus());
    }
}
