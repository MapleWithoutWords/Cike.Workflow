namespace Cike.Workflow.Expression.Liquid.Expressions;

internal class LiquidExpressionHandler(ILiquidTemplateManager liquidTemplateManager) : IExpressionHandler
{
    public async ValueTask<object?> EvaluateAsync(Workflow.Expressions.Models.Expression expression, Type returnType, ExpressionExecutionContext context, ExpressionEvaluatorOptions options)
    {
        var liquidExpression = expression.Value.ConvertTo<string>() ?? "";
        var renderedString = await liquidTemplateManager.RenderAsync(liquidExpression, context);
        return renderedString.ConvertTo(returnType);
    }
}
