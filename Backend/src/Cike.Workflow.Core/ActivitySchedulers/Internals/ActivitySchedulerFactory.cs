using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivitySchedulers.Internals;

internal class ActivitySchedulerFactory : IActivitySchedulerFactory, ISingletonDependency
{
    public IActivityScheduler CreateScheduler() => new QueueBasedActivityScheduler();
}
