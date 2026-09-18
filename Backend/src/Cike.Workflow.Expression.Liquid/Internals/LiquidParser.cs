namespace Cike.Workflow.Expression.Liquid.Internals;

/// <summary>
/// A parser for the Liquid templating engine.
/// </summary>
public class LiquidParser : FluidParser, IScopedDependency
{
    /// <inheritdoc />
    public LiquidParser(IOptions<FluidOptions> options)
    {
        foreach (var configuration in options.Value.ParserConfiguration)
            configuration(this);
    }
}
