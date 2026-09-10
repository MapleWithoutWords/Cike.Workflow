using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Serialization;

public interface IActivitySerializer
{
    string Serialize(IActivity activity);

    string Serialize(object value);

    IActivity Deserialize(string serializedActivity);

    object Deserialize(string serializedValue, Type type);

    T Deserialize<T>(string serializedValue);
}
