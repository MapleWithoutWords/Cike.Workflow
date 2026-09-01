using Cike.Workflow.Core.Schedulers;

namespace Cike.Workflow.Core.Schedulers.Internals;

internal class ActivitySchedulerFactory : IActivitySchedulerFactory, ISingletonDependency
{
    public IActivityScheduler CreateScheduler() => new QueueBasedActivityScheduler();
}
