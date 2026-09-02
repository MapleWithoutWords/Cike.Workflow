using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Workflow.Core.Exceptions;

public class ActivityNotFoundException : Exception
{
    /// <inheritdoc />
    public ActivityNotFoundException(string missingTypeName) : base($"Activity type '{missingTypeName}' could not be found.")
    {
        MissingTypeName = missingTypeName;
    }

    /// <inheritdoc />
    public ActivityNotFoundException(string missingTypeName, int missingTypeVersion) : base($"Activity type '{missingTypeName}' version '{missingTypeVersion}' could not be found.")
    {
        MissingTypeName = missingTypeName;
        MissingTypeVersion = missingTypeVersion;
    }

    /// <summary>
    /// The type name of the missing activity type.
    /// </summary>
    public string MissingTypeName { get; }

    /// <summary>
    /// The version of the missing activity type.
    /// </summary>
    public int MissingTypeVersion { get; }
}
