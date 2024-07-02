namespace SnowflakeId.AutoRegister.Core;

public class ExtendLifeTimeTask(TimeSpan interval, Func<CancellationToken, Task> operation)
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public void Start()
    {
        Task.Factory.StartNew(() => RunPeriodicAsync(_cancellationTokenSource.Token),
            _cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task RunPeriodicAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await operation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation here if necessary
                break;
            }

            // Wait for the next interval, respecting the cancellation token
            try
            {
                await Task.Delay(interval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, break the loop
                break;
            }
        }
    }
}