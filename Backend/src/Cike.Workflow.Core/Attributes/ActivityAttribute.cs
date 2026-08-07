using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
public class ActivityAttribute : Attribute
{
    public ActivityAttribute()
    {
        // Default constructor.
    }

    public ActivityAttribute(string @namespace, string? category, string? description = null)
    {
        Namespace = @namespace;
        Description = description;
        Category = category;
    }

    public ActivityAttribute(string @namespace, string? description = null)
    {
        Namespace = @namespace;
        Description = description;
        Category = @namespace;
    }

    public ActivityAttribute(string @namespace, string? type, int version = 1, string? description = null, string? category = null)
    {
        Namespace = @namespace;
        Type = type;
        Version = version;
        Description = description;
        Category = category;
    }

    public string? Namespace { get; set; }
    public string? Type { get; set; }
    public int Version { get; set; } = 1;
    public string? Description { get; set; }
    public string? DisplayName { get; set; }
    public string? Category { get; set; }
}
