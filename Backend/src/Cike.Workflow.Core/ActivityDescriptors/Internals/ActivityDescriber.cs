using Cike.Workflow.Core.Helpers;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Cike.Workflow.Core.ActivityDescriptors.Internals;

internal class ActivityDescriber : IActivityDescriber, ISingletonDependency
{
    public async Task<ActivityDescriptor> DescribeActivityAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type activityType, CancellationToken cancellationToken = default)
    {
        var activityAttr = activityType.GetCustomAttribute<ActivityAttribute>();
        var ns = activityAttr?.Namespace ?? ActivityTypeNameHelper.GenerateNamespace(activityType) ?? "Cike";
        var friendlyName = GetFriendlyActivityName(activityType);
        var typeName = activityAttr?.Type ?? friendlyName;
        var typeVersion = activityAttr?.Version ?? 1;
        var fullTypeName = ActivityTypeNameHelper.GenerateTypeName(activityType);
        var displayName = activityType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? activityAttr?.DisplayName ?? friendlyName.Humanize(LetterCasing.Title);
        var description = activityType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? activityAttr?.Description;
        var isTerminal = activityType.FindInterfaces((type, criteria) => type == typeof(ITerminalNode), null).Any();
        var isStart = activityType.FindInterfaces((type, criteria) => type == typeof(IStartNode), null).Any();

        var descriptor = new ActivityDescriptor
        {
            TypeName = fullTypeName,
            ClrType = activityType,
            Namespace = ns,
            Name = typeName,
            Description = description,
            Version = typeVersion,
            DisplayName = displayName,
            Inputs = (await DescribeInputPropertiesAsync(activityType, cancellationToken)).ToList(),
            Outputs = (await DescribeOutputPropertiesAsync(activityType, cancellationToken)).ToList(),
            IsContainer = typeof(ContainerActivity).IsAssignableFrom(activityType),
            IsStart = isStart,
            IsTerminal = isTerminal,
        };

        // If the activity has a default output, set its IsSerializable property to the value of the OutputAttribute.IsSerializable property.
        var outputAttribute = activityType.GetCustomAttribute<OutputAttribute>();
        var defaultOutputDescriptor = descriptor.Outputs.FirstOrDefault(x => x.Name == nameof(IActivityWithResult.Result);

        if (defaultOutputDescriptor != null)
        {
            var isResultSerializable = outputAttribute?.IsSerializable;
            defaultOutputDescriptor.IsSerializable = isResultSerializable;
        }

        return descriptor;
    }

    public async Task<IEnumerable<InputDescriptor>> DescribeInputPropertiesAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type activityType, CancellationToken cancellationToken = default)
    {
        var inputProperties = activityType.GetProperties().Where(x => typeof(Input).IsAssignableFrom(x.PropertyType) || x.GetCustomAttribute<InputAttribute>() != null).DistinctBy(x => x.Name);
        return await Task.WhenAll(inputProperties.Select(async x => await DescribeInputPropertyAsync(x, cancellationToken)));
    }

    public async Task<IEnumerable<OutputDescriptor>> DescribeOutputPropertiesAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type activityType, CancellationToken cancellationToken = default)
    {
        var outputProperties = activityType.GetProperties().Where(x => typeof(Output).IsAssignableFrom(x.PropertyType)).DistinctBy(x => x.Name).ToList();
        return await Task.WhenAll(outputProperties.Select(async x => await DescribeOutputPropertyAsync(x, cancellationToken)));
    }

    private static string GetFriendlyActivityName(Type t)
    {
        if (!t.IsGenericType)
            return t.Name;
        var baseName = t.Name.Substring(0, t.Name.IndexOf('`'));
        var argNames = string.Join(", ", t.GetGenericArguments().Select(a => a.Name));
        return $"{baseName}<{argNames}>";
    }

    private Task<OutputDescriptor> DescribeOutputPropertyAsync(PropertyInfo propertyInfo, CancellationToken cancellationToken = default)
    {
        var outputAttribute = propertyInfo.GetCustomAttribute<OutputAttribute>();
        var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
        var typeArgs = propertyInfo.PropertyType.GenericTypeArguments;
        var wrappedPropertyType = typeArgs.Any() ? typeArgs[0] : typeof(object);

        return Task.FromResult(new OutputDescriptor((outputAttribute?.Name ?? propertyInfo.Name).Pascalize(), outputAttribute?.DisplayName ?? propertyInfo.Name.Humanize(LetterCasing.Title), wrappedPropertyType, propertyInfo.GetValue, propertyInfo.SetValue, outputAttribute?.IsSerializable));
    }

    private async Task<InputDescriptor> DescribeInputPropertyAsync(PropertyInfo propertyInfo, CancellationToken cancellationToken = default)
    {
        var inputAttribute = propertyInfo.GetCustomAttribute<InputAttribute>();
        var propertyType = propertyInfo.PropertyType;
        var isWrappedProperty = typeof(Input).IsAssignableFrom(propertyType);
        var autoEvaluate = inputAttribute?.AutoEvaluate ?? true;
        var wrappedPropertyType = !isWrappedProperty ? propertyType : propertyInfo.PropertyType.GenericTypeArguments[0];

        if (wrappedPropertyType.IsNullableType())
            wrappedPropertyType = wrappedPropertyType.GetTypeOfNullable();

        return new(
            inputAttribute?.Name ?? propertyInfo.Name,
            wrappedPropertyType,
            propertyInfo.GetValue,
            propertyInfo.SetValue,
            isWrappedProperty,
            inputAttribute?.DisplayName ?? propertyInfo.Name.Humanize(LetterCasing.Title),
            inputAttribute?.IsSerializable ?? true,
            autoEvaluate,
            inputAttribute?.EvaluatorType?.FullName
        );
    }
}
