using System.Net;
using System.Text.Json;

namespace Cike.Workflow.Service.Open.Tests.WorkflowDefinitions;

/// <summary>
/// 画布校验端点：POST /api/v1/WorkflowDefinitions/ValidateCanvas。
/// 校验不落库、不产生版本，返回结构化错误列表（空列表即通过）。
/// </summary>
[Category("Integration")]
public class WorkflowCanvasValidationTest : WorkflowDefinitionTestBase
{
    private const string Endpoint = "/api/v1/WorkflowDefinitions/ValidateCanvas";

    [Test]
    public async Task ValidateCanvas_WithOrphanNode_ReturnsStructuredErrorsWithActivityId()
    {
        var response = await CreateClient().PostAsJsonAsync(Endpoint, new
        {
            root = new
            {
                type = "Cike.Flowchart",
                id = "v_flowchart",
                activities = new object[]
                {
                    new { type = "Cike.Start", id = "v_start" },
                    new { type = "Cike.End", id = "v_orphan" },
                },
                connections = Array.Empty<object>(),
            },
            options = new { variables = Array.Empty<object>() },
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var errors = await ReadErrorsAsync(response);
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(GetString(errors[0], "activityId"), Is.EqualTo("v_orphan"));
        Assert.That(GetString(errors[0], "message"), Does.Contain("孤立节点"));
    }

    [Test]
    public async Task ValidateCanvas_WithValidCanvas_ReturnsEmptyErrors()
    {
        var response = await CreateClient().PostAsJsonAsync(Endpoint, new
        {
            root = CreateValidCanvas("validate"),
            options = new
            {
                variables = new object[] { new { id = "var_1", name = "Amount", typeName = "Text", isArray = false } },
            },
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var errors = await ReadErrorsAsync(response);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public async Task ValidateCanvas_WithInvalidVariableDefinition_ReturnsErrorWithoutActivityId()
    {
        var response = await CreateClient().PostAsJsonAsync(Endpoint, new
        {
            root = CreateValidCanvas("validate"),
            options = new
            {
                variables = new object[]
                {
                    new { id = "var_1", name = "", typeName = "Text", isArray = false },
                    new { id = "var_2", name = "Amount", typeName = "Text", isArray = false },
                    new { id = "var_3", name = "Amount", typeName = "Text", isArray = false },
                },
            },
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var errors = await ReadErrorsAsync(response);
        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors.Select(x => x.GetProperty("activityId").ValueKind), Has.All.EqualTo(JsonValueKind.Null));
    }

    [Test]
    public async Task ValidateCanvas_WithNullRoot_ReturnsBadRequest()
    {
        var response = await CreateClient().PostAsJsonAsync(Endpoint, new
        {
            options = new { variables = Array.Empty<object>() },
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static async Task<List<JsonElement>> ReadErrorsAsync(HttpResponseMessage response)
    {
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement.EnumerateArray().ToList();
    }
}
