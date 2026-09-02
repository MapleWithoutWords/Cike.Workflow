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

    [JsonPropertyName("typeName")]
    public Type Type { get; set; } = null!;

    public string? DisplayName { get; set; }

    public bool? IsSerializable { get; set; }

    [JsonIgnore]
    public Func<IActivity, object?> ValueGetter { get; set; } = null!;

    [JsonIgnore]
    public Action<IActivity, object?> ValueSetter { get; set; } = null!;
}
