using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Cike.Workflow.Expressions.Extensions;

/// <summary>
/// Adds extension methods to <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets a friendly type name for the specified type.
    /// </summary>
    public static string GetFriendlyTypeName(this Type type, Brackets brackets)
    {
        if (type.IsArray)
        {
            var elementTypeName = GetFriendlyTypeName(type.GetElementType()!, brackets);
            var rank = type.GetArrayRank();
            var commas = rank > 1 ? new string(',', rank - 1) : string.Empty;
            return elementTypeName + "[" + commas + "]";
        }

        if (!type.IsGenericType)
            return type.FullName!;

        var sb = new StringBuilder();
        sb.Append(type.Namespace);
        sb.Append('.');
        sb.Append(type.Name[..type.Name.IndexOf('`')]);
        sb.Append(brackets.Open);
        var genericArgs = type.GetGenericArguments();
        for (var i = 0; i < genericArgs.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(GetFriendlyTypeName(genericArgs[i], brackets));
        }

        sb.Append(brackets.Close);
        return sb.ToString();
    }

}
