using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.ActivityDescriptors.Models;

public abstract class PropertyDescriptor
{
    public string Name { get; set; } = null!;

    public string ClrName { get; set; } = null!;

    [JsonIgnore]
    public Type Type { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public bool? IsSerializable { get; set; }

    public bool IsBrowsable { get; set; } = true;

    [JsonIgnore]
    public Func<IActivity, object?> ValueGetter { get; set; } = null!;

    [JsonIgnore]
    public Action<IActivity, object?> ValueSetter { get; set; } = null!;
}
