using Cike.Workflow.Expressions.Internals;

namespace Cike.Workflow.Core.Expressions
{
    internal class VariableExpressionHandler : IExpressionHandler
    {
        public ValueTask<object?> EvaluateAsync(Expression expression, Type returnType, ExpressionExecutionContext context, ExpressionEvaluatorOptions options)
        {
            object? result = null;

            if (expression.Value is string variableName)
            {
                result = context.GetVariable(variableName)?.Get(context);
            }
            else if (expression.Value is Variable variable)
            {
                result = variable?.Get(context);
            }

            return ValueTask.FromResult(result);
        }
    }
}
