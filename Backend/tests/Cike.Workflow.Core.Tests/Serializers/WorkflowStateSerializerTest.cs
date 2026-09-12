using Cike.Workflow.Common.Serialization.Internals;
using Cike.Workflow.Core.Enums;
using Cike.Workflow.Core.Runners.Models;
using Cike.Workflow.Core.Serialization.Internals;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cike.Workflow.Core.Tests.Serializers;

/// <summary>
/// WorkflowState 序列化器回归测试：GetOptions 在首次与后续调用间必须返回一致的 options
/// （均带引用保留 handler），否则首次序列化产物（$values 引用格式）在后续反序列化时无法读取。
/// </summary>
internal class WorkflowStateSerializerTest
{
    [Test]
    public void Serialize_ThenDeserialize_AcrossTwoOptionsCalls_RestoresState()
    {
        var serializer = new JsonWorkflowStateSerializer(SerializationTypeRegistry.CreateDefault(), NullLoggerFactory.Instance);
        var state = new WorkflowState
        {
            DefinitionId = "WF_test",
            Status = WorkflowStatus.Suspended,
            Input = new Dictionary<string, object> { ["key"] = "value" },
        };

        // Serialize 走首次 GetOptions 调用，Deserialize 走后续调用——两条路径必须读同一格式
        var json = serializer.Serialize(state);
        var restored = serializer.Deserialize<WorkflowState>(json);

        Assert.That(restored.Status, Is.EqualTo(WorkflowStatus.Suspended));
        Assert.That(restored.Input, Contains.Key("key"));
        Assert.That(restored.Input["key"], Is.EqualTo("value"));
    }
}
