namespace Cike.Workflow.Expression.Javascript.Extensions;

public static class EngineOptionsExtensions
{
    public static Jint.Options RegisterType<T>(this Jint.Options options) => options.RegisterType(typeof(T));

    public static Jint.Options RegisterType(this Jint.Options options, Type type)
    {
        var name = type.Name;

        if (!IsUsableAsIdentifier(name))
            return options;

        return options.AddLazyGlobal(name, engine => TypeReference.CreateTypeReference(engine, type));
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
