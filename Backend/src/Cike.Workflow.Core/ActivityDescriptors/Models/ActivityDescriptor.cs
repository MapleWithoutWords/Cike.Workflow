using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivityDescriptors.Models;

public class ActivityDescriptor
{
    public Guid? TenantId { get; set; }

    public string TypeName { get; set; } = null!;

    public Type ClrType { get; set; } = null!;

    public string Namespace { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Version { get; set; }

    public string Category { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public ICollection<InputDescriptor> Inputs { get; init; } = new List<InputDescriptor>();

    public ICollection<OutputDescriptor> Outputs { get; init; } = new List<OutputDescriptor>();

    public bool IsContainer { get; set; }

    public bool IsStart { get; set; }

    public bool IsTerminal { get; set; }
}
