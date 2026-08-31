namespace Cike.Workflow.Core.Models;

public delegate ValueTask ExecuteActivityDelegate(ActivityExecutionContext context);

public delegate ValueTask ActivityCompletionCallback(ActivityCompletedContext context);
