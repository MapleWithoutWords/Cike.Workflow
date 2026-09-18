using Cike.Workflow.Expression.Javascript.Helpers;
using Cike.Workflow.Expression.Javascript.Options;
using Jint.Runtime.Interop;
using Microsoft.Extensions.Options;

namespace Cike.Workflow.Expression.Javascript.Extensions;

public static class EngineExtensions
{
    public static void RegisterType<T>(this Engine engine) => engine.RegisterType(typeof(T));

    public static void RegisterType(this Engine engine, Type type)
    {
        var name = type.Name;

        if (!IsUsableAsIdentifier(name))
            return;

        // Leave any value that the host or JavaScript already installed under this name untouched.
        if (engine.Global.HasOwnProperty(name))
            return;

        engine.SetValue(name, TypeReference.CreateTypeReference(engine, type));
    }

    private static bool IsUsableAsIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name) || (!char.IsLetter(name[0]) && name[0] != '_' && name[0] != '$'))
            return false;

        foreach (var c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '$')
                return false;
        }

        return true;
    }
}
