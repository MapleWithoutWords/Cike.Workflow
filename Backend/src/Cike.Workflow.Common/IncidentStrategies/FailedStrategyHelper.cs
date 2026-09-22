namespace Cike.Workflow.Common.IncidentStrategies;

public static class FailedStrategyHelper
{
    public static async ValueTask ExecuteAsync(Func<Task> action, Action<Exception> errorHandler, int interval = 0, int retryCount = 1)
    {
        Exception? exception = null;
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                await action();
                exception = null;
            }
            catch (Exception ex)
            {
                exception = ex;
                if (i < retryCount - 1)
                    await Task.Delay(interval);
            }
        }
        if (exception != null)
        {
            errorHandler(exception);
        }
    }
}
