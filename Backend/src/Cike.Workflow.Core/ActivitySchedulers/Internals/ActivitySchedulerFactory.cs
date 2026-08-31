namespace Cike.Workflow.Core.ActivitySchedulers.Internals;

internal class ActivitySchedulerFactory : IActivitySchedulerFactory, ISingletonDependency
{
    public IActivityScheduler CreateScheduler() => new QueueBasedActivityScheduler();
}
