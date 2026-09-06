using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cike.Workflow.Expressions.Models;

public class Expression
{
    [JsonConstructor]
    public Expression()
    {
    }

    public Expression(string type, object? value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; set; } = default!;

    public object? Value { get; set; }


    public static Expression LiteralExpression(object? value) => new("Literal", value);
}
