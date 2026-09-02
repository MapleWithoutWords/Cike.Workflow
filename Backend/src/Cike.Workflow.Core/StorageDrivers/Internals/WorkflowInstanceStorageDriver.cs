using Cike.Core.DependencyInjection;
using Cike.Workflow.Core.StorageDrivers.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.StorageDrivers.Internals;

internal class WorkflowInstanceStorageDriver(ILogger<WorkflowInstanceStorageDriver> logger) : IStorageDriver, ITransientDependency
{
    public const string VARIABLES_DICTIONARY_STATE_KEY = "Variables";

    /// <inheritdoc />
    public ValueTask WriteAsync(string id, object value, StorageDriverContext context)
    {
        UpdateVariablesDictionary(context, dictionary =>
        {
            try
            {
                var node = JsonSerializer.SerializeToNode(value);
                dictionary[id] = node!;
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException or ObjectDisposedException)
            {
                logger.LogWarning(ex, "Failed to serialize variable '{VariableId}' of type '{VariableType}' for workflow instance storage. The variable will be skipped.",
                    id, value?.GetType().FullName ?? "null");

                dictionary.Remove(id);
            }
        });
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<object?> ReadAsync(string id, StorageDriverContext context)
    {
        var dictionary = GetVariablesDictionary(context);
        var node = dictionary.GetValueOrDefault(id);
        var variable = context.Variable;
        var variableType = variable.GetVariableType();
        var options = new ObjectConverterOptions
        {
            DeserializeJsonObjectToObject = true,
            //SerializerOptions = payloadSerializer.GetOptions()
        };
        var result = node.TryConvertTo(variableType, options);
        var parsedValue = result.IsSuccess ? result.Value : node;
        return new(parsedValue);
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(string id, StorageDriverContext context)
    {
        UpdateVariablesDictionary(context, dictionary => dictionary.Remove(id));
        return ValueTask.CompletedTask;
    }

    private VariablesDictionary GetVariablesDictionary(StorageDriverContext context) => context.ExecutionContext.Properties.GetOrAdd(VARIABLES_DICTIONARY_STATE_KEY, () => new VariablesDictionary());
    private void SetVariablesDictionary(StorageDriverContext context, VariablesDictionary dictionary) => context.ExecutionContext.Properties[VARIABLES_DICTIONARY_STATE_KEY] = dictionary;

    private void UpdateVariablesDictionary(StorageDriverContext context, Action<VariablesDictionary> update)
    {
        var dictionary = GetVariablesDictionary(context);
        update(dictionary);
        SetVariablesDictionary(context, dictionary);
    }
}

public class VariablesDictionary : Dictionary<string, JsonNode>;
