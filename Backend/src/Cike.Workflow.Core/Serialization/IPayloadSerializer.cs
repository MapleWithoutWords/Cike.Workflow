using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Serialization;

public interface IPayloadSerializer
{
    string Serialize(object payload);

    JsonElement SerializeToElement(object payload);

    object Deserialize(string serializedData);

    object Deserialize(string serializedData, Type type);

    object Deserialize(JsonElement serializedData);

    T Deserialize<T>(string serializedData);

    T Deserialize<T>(JsonElement serializedData);

    JsonSerializerOptions GetOptions();
}
