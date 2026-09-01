using System.Net.NetworkInformation;

namespace Cike.Workflow.Core.Enums;

public enum ActivityStatus
{
    Pending,

    Running,

    Completed,

    Canceled,

    Faulted
}

public static class ActivityStatusExtensions
{
    public static bool CanCancelActivity(this ActivityStatus status)
    {
        return status is not ActivityStatus.Canceled and not ActivityStatus.Completed;
    }
}
