using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Serialization;

public interface IJsonSerializer
{
    JsonSerializerOptions GetOptions();

    void ApplyOptions(JsonSerializerOptions options);

    [RequiresUnreferencedCode("The type is not known at compile time.")]
    string Serialize(object value);

    [RequiresUnreferencedCode("The type is not known at compile time.")]
    string Serialize(object value, Type type);

    string Serialize<T>(T value);

    [RequiresUnreferencedCode("The type is not known at compile time.")]
    object Deserialize(string json);

    [RequiresUnreferencedCode("The type is not known at compile time.")]
    object Deserialize(string json, Type type);

    T Deserialize<T>(string json);
}
