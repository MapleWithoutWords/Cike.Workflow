using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cike.Workflow.Expressions.Models;

/// <summary>
/// Describes an expression type.
/// </summary>
public class ExpressionDescriptor
{
    public ExpressionDescriptor()
    {
    }

    /// <summary>
    /// Gets or sets the syntax name.
    /// </summary>
    public string Type { get; init; } = default!;

    /// <summary>
    /// Gets or sets the display name of the expression type.
    /// </summary>
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the lucide icon name for frontend display, e.g. "type".
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the expression handler factory.
    /// </summary>
    [JsonIgnore]
    public Func<IServiceProvider, IExpressionHandler> HandlerFactory { get; set; } = default!;
}
