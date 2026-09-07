using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Domain.Data.ValueObjects;

public class VariableDefinition
{
    public VariableDefinition()
    {

    }

    public VariableDefinition(string id, string name, string typeName, bool isArray, string? defaultValue, string? storageDriverType)
    {
        Id = id;
        Name = name;
        TypeName = typeName;
        IsArray = isArray;
        DefaultValue = defaultValue;
        StorageDriverType = storageDriverType;
    }

    public string Id { get; set; }

    public string Name { get; set; } = null!;

    public string TypeName { get; set; } = "object";

    public bool IsArray { get; set; }

    public string? DefaultValue { get; set; }

    public string? StorageDriverType { get; set; }
}
