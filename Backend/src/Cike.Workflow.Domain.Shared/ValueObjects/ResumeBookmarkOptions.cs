using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Shared.ValueObjects;

public class ResumeBookmarkOptionsValueObject
{
    /// <summary>
    /// The input to provide to the workflow.
    /// </summary>
    public IDictionary<string, object>? Input { get; set; }

    /// <summary>
    /// The properties to provide to the workflow.
    /// </summary>
    public IDictionary<string, object>? Properties { get; set; }
}
