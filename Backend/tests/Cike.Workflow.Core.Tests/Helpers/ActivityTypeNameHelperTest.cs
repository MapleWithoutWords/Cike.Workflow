using Cike.Workflow.Core.Activities;
using Cike.Workflow.Core.Activities.Abstracts;
using Cike.Workflow.Core.Attributes;
using Cike.Workflow.Core.Helpers;

namespace Cike.Workflow.Core.Tests.Helpers;

[TestFixture]
public class ActivityTypeNameHelperTest
{
    [Test]
    public void GenerateNamespace_WithActivityAttribute_ReturnsAttributeNamespace()
    {
        var ns = ActivityTypeNameHelper.GenerateNamespace(typeof(TestActivityWithAttribute));
        Assert.That(ns, Is.EqualTo("Custom.Namespace"));
    }

    [Test]
    public void GenerateNamespace_WithoutActivityAttribute_ReturnsClrNamespace()
    {
        // WriteLine has [Activity("Cike", ...)] so its namespace comes from the attribute
        var ns = ActivityTypeNameHelper.GenerateNamespace(typeof(WriteLine));
        Assert.That(ns, Is.EqualTo("Cike"));
    }

    [Test]
    public void GenerateTypeName_WithActivityAttribute_UsesAttributeType()
    {
        var typeName = ActivityTypeNameHelper.GenerateTypeName(typeof(TestActivityWithAttribute));
        Assert.That(typeName, Does.Contain("CustomType"));
    }

    [Test]
    public void GenerateTypeName_WithoutActivityAttribute_UsesClassName()
    {
        var typeName = ActivityTypeNameHelper.GenerateTypeName(typeof(WriteLine));
        Assert.That(typeName, Does.Contain("WriteLine"));
    }

    [Test]
    public void GenerateTypeName_GenericType_FormatsCorrectly()
    {
        var typeName = ActivityTypeNameHelper.GenerateTypeName(typeof(TestGenericActivity<string>));
        Assert.That(typeName, Does.Contain("TestGenericActivity"));
        Assert.That(typeName, Does.Contain("String"));
    }

    [Test]
    public void GenerateTypeName_NonGeneric_IncludesNamespace()
    {
        var typeName = ActivityTypeNameHelper.GenerateTypeName(typeof(WriteLine));
        // WriteLine has [Activity("Cike", ...)] so the type name starts with "Cike."
        Assert.That(typeName, Does.StartWith("Cike."));
    }

    [Test]
    public void GetCategoryFromNamespace_WithDottedNamespace_ReturnsLastSegment()
    {
        var category = ActivityTypeNameHelper.GetCategoryFromNamespace("Cike.Workflow.Activities");
        Assert.That(category, Is.EqualTo("Activities"));
    }

    [Test]
    public void GetCategoryFromNamespace_WithSingleSegment_ReturnsSameSegment()
    {
        var category = ActivityTypeNameHelper.GetCategoryFromNamespace("Workflows");
        Assert.That(category, Is.EqualTo("Workflows"));
    }

    [Test]
    public void GetCategoryFromNamespace_WithNullOrEmpty_ReturnsNull()
    {
        Assert.That(ActivityTypeNameHelper.GetCategoryFromNamespace(null), Is.Null);
        Assert.That(ActivityTypeNameHelper.GetCategoryFromNamespace(""), Is.Null);
        Assert.That(ActivityTypeNameHelper.GetCategoryFromNamespace("  "), Is.Null);
    }

    [Activity("Custom.Namespace", "Category", "Description", Type = "CustomType")]
    private class TestActivityWithAttribute : Activity
    {
    }
}

[Activity("Cike", "Test", "A generic test activity")]
public class TestGenericActivity<T> : Activity
{
}
