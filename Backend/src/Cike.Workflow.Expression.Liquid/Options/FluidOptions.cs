namespace Cike.Workflow.Expression.Liquid.Options
{
    public class FluidOptions
    {
        public Dictionary<string, Type> FilterRegistrations { get; } = new();

        public IList<Action<LiquidParser>> ParserConfiguration { get; } = new List<Action<LiquidParser>>();

        public TextEncoder Encoder { get; set; } = NullEncoder.Default;

        public Action<TemplateContext> ConfigureFilters { get; set; } = _ => { };
    }
}
