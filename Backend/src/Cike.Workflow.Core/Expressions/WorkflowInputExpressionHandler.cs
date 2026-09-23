using Cike.Workflow.Expressions.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Expressions;

internal class WorkflowInputExpressionHandler : IExpressionHandler
{
    public ValueTask<object?> EvaluateAsync(Expression expression, Type returnType, ExpressionExecutionContext context, ExpressionEvaluatorOptions options)
    {
        object? result = null;

        if (expression.Value is string inputName)
        {
            result = context.GetInput(inputName);
        }
        else if (expression.Value is InputDefinition inputDefinition)
        {
            result = context.GetInput(inputDefinition.Name);
        }

        return ValueTask.FromResult(result);
    }
}
