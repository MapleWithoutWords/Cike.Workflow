using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Helpers;

public static class ExpressionExecutionContextHelper
{
    /// <summary>
    /// The key used to store the <see cref="WorkflowExecutionContext"/> in the <see cref="ExpressionExecutionContext.TransientProperties"/> dictionary.
    /// </summary>
    public static readonly object WorkflowExecutionContextKey = new();

    /// <summary>
    /// The key used to store the <see cref="ActivityExecutionContext"/> in the <see cref="ExpressionExecutionContext.TransientProperties"/> dictionary.
    /// </summary>
    public static readonly object ActivityExecutionContextKey = new();

    /// <summary>
    /// The key used to store the input in the <see cref="ExpressionExecutionContext.TransientProperties"/> dictionary.
    /// </summary>
    public static readonly object InputKey = new();

    /// <summary>
    /// The key used to store the workflow in the <see cref="ExpressionExecutionContext.TransientProperties"/> dictionary.
    /// </summary>
    public static readonly object WorkflowKey = new();

    /// <summary>
    /// The key used to store the activity in the <see cref="ExpressionExecutionContext.TransientProperties"/> dictionary.
    /// </summary>
    public static readonly object ActivityKey = new();

    public static IDictionary<object, object> CreateActivityExecutionContextPropertiesFrom(WorkflowExecutionContext workflowExecutionContext, IDictionary<string, object> input) =>
        new Dictionary<object, object>
        {
            [WorkflowExecutionContextKey] = workflowExecutionContext,
            [InputKey] = input,
            [WorkflowKey] = workflowExecutionContext.WorkflowGraph.Workflow,
        };
}
