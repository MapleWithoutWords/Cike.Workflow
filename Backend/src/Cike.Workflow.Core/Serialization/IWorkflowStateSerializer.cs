using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Serialization;

public interface IWorkflowStateSerializer
{
    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The serialization process may require access to the type.")]
    string Serialize(WorkflowState workflowState);

    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The serialization process may require access to the type.")]
    byte[] SerializeToUtfBytes(WorkflowState workflowState);

    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The serialization process may require access to the type.")]
    JsonElement SerializeToElement(WorkflowState workflowState);

    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The serialization process may require access to the type.")]
    string Serialize(object workflowState);

    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The deserialization process may require access to the type.")]
    WorkflowState Deserialize(string serializedState);

    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The deserialization process may require access to the type.")]
    WorkflowState Deserialize(JsonElement serializedState);

    [RequiresUnreferencedCode("The type 'T' may be trimmed from the output. The deserialization process may require access to the type.")]
    T Deserialize<T>(string serializedState);
}
