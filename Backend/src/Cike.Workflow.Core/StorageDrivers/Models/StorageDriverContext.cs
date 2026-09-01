using Cike.Workflow.Core.Variables;

namespace Cike.Workflow.Core.StorageDrivers.Models;

/// <summary>
/// Provides context for storage drivers.
/// </summary>
public record StorageDriverContext(IExecutionContext ExecutionContext, Variable Variable, CancellationToken CancellationToken);
