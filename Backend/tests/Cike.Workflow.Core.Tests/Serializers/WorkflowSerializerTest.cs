using Cike.Workflow.Common.Serialization;
using Cike.Workflow.Common.Serialization.Converters;
using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.FlowchartActivity;
using Cike.Workflow.Core.Activities.FlowchartActivity.Models;
using Cike.Workflow.Core.ActivityDescriptors;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.Workflow.Core.Tests.Serializers;

internal class WorkflowSerializerTest : BaseIntegrationTest
{
    [Test]
    public void TestSerializeWorkflow()
    {
        var fc2Start = new Start { Id = "fc2_s" };
        var fc2If = new If { Id = "fc2_if", Condition = new(false) };
        var fc2True = new WriteLine("FC2-True") { Id = "fc2_t" };
        var fc2False = new WriteLine("FC2-False") { Id = "fc2_f" };
        var fc2End = new End { Id = "fc2_e" };

        var flowchart2 = new Flowchart
        {
            Activities = { fc2Start, fc2If, fc2True, fc2False, fc2End },
            Connections =
            {
                new ActivityConnection(new ActivityEndpoint("fc2_s"), new ActivityEndpoint("fc2_if")),
                new ActivityConnection(new ActivityEndpoint("fc2_if", "True"), new ActivityEndpoint("fc2_t")),
                new ActivityConnection(new ActivityEndpoint("fc2_if", "False"), new ActivityEndpoint("fc2_f")),
                new ActivityConnection(new ActivityEndpoint("fc2_t"), new ActivityEndpoint("fc2_e")),
                new ActivityConnection(new ActivityEndpoint("fc2_f"), new ActivityEndpoint("fc2_e"))
            },
            Id = "flowchart-1",
            Start = null
        };
        var jsonOptions = JsonHelper.CreateOptionsInternal();
        var activityRegister = serviceProvider.GetService<IActivityRegistry>();
        var serializationTypeRegistry = serviceProvider.GetService<ISerializationTypeRegistry>();
        jsonOptions.Converters.Add(new ActivityJsonConverter(activityRegister));
        //jsonOptions.Converters.Add(new TypeJsonConverter(serializationTypeRegistry));
        var jsonStr = JsonHelper.Serialize(flowchart2, jsonOptions);

        var deserializedFlowchart = JsonHelper.Deserialize<Flowchart>(jsonStr, jsonOptions);
    }
}
