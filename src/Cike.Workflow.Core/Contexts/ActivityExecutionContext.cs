using System;
using System.Collections.Generic;
using System.Text;
using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Memory;
using Elsa.Expressions.Models;

namespace Cike.Workflow.Core.Contexts;

public class ActivityExecutionContext: IExecutionContext, IDisposable
{
    public ExpressionExecutionContext ExpressionExecutionContext { get; } = null!;

    public required string Id { get; set; }

    public required IActivity Activity { get; set; }

    public IEnumerable<Variable> Variables { get; set; } = new List<Variable>();

    public void Dispose()
    {

    }
}
