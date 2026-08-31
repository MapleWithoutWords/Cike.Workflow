using Cike.Workflow.Core.ActivitySchedulers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivitySchedulers;

public interface IActivityScheduler
{
    bool HasAny { get; }

    void Schedule(ActivityWorkItem workItem);

    ActivityWorkItem Take();

    IEnumerable<ActivityWorkItem> List();

    bool Any(Func<ActivityWorkItem, bool> predicate);

    ActivityWorkItem? Find(Func<ActivityWorkItem, bool> predicate);

    int RemoveWhere(Func<ActivityWorkItem, bool> predicate);

    void Clear();
}
