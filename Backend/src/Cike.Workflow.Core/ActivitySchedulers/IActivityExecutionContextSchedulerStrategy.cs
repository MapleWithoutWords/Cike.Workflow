using Cike.Workflow.Core.ActivitySchedulers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivitySchedulers;

public interface IActivityExecutionContextSchedulerStrategy
{
    Task ScheduleActivityAsync(
        ActivityExecutionContext context,
        IActivity? activity,
        ActivityExecutionContext? owner,
        ScheduleWorkOptions? options = null);

    Task ScheduleActivityAsync(
        ActivityExecutionContext context,
        ActivityNode? activityNode,
        ActivityExecutionContext? owner = null,
        ScheduleWorkOptions? options = null);
}
