using Cike.EventBus.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Runtime.Internals.Commands;

public record DispatcherWorkflowInstanceCommand(DispatchWorkflowInstanceRequest Request) : BackgroundEvent
{
}
