using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Expression.Liquid;

public interface ILiquidTemplateManager
{
    /// <summary>
    /// Renders a Liquid template as a <see cref="string"/>.
    /// </summary>
    Task<string?> RenderAsync(string template, ExpressionExecutionContext expressionExecutionContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a Liquid template.
    /// </summary>
    bool Validate(string template, out string error);
}
