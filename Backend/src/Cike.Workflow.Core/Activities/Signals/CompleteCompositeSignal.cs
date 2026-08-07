using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Activities.Signals;

public record CompleteCompositeSignal(object? Value = default);
