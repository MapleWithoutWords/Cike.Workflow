namespace Cike.Workflow.Core.Tests.Helpers;

/// <summary>
/// Test expression factory. The Cike.Workflow.Expression.Liquid/.Javascript projects turn
/// Cike.Workflow.Expression into an enclosing-namespace member, which shadows the Expression class
/// imported via usings; this factory wraps the fully-qualified construction so tests stay terse.
/// </summary>
public static class TestExpressions
{
    public static Cike.Workflow.Expressions.Models.Expression Literal(object? value) => new("Literal", value);

    public static Cike.Workflow.Expressions.Models.Expression Of(string type, object? value) => new(type, value);
}
