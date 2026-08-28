using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Enums;

public enum WorkflowStatus
{
    Pending,

    Executing,

    Suspended,

    Finished,

    Cancelled,

    Faulted,

    Interrupted,
}
