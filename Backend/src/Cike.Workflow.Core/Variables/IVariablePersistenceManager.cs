using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Variables;

public interface IVariablePersistenceManager
{
    Task LoadVariablesAsync(WorkflowExecutionContext context);

    Task SaveVariablesAsync(WorkflowExecutionContext context);

    Task DeleteVariablesAsync(ActivityExecutionContext context);

    Task DeleteVariablesAsync(WorkflowExecutionContext context);
}
