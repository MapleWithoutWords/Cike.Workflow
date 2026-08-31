using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Activities.Abstracts;

public interface IInitializable : IActivity
{
    /// <summary>
    /// Called by the system to initialize the activity.
    /// </summary>
    ValueTask InitializeAsync(InitializationContext context);
}

/// <summary>
/// Provides access to contextual services and information.
/// </summary>
public record InitializationContext(IServiceProvider ServiceProvider, CancellationToken CancellationToken);
