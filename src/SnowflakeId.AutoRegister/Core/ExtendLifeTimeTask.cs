namespace SnowflakeId.AutoRegister.Core;

public class ExtendLifeTimeTask(TimeSpan interval, Func<CancellationToken, Task> operation)
{
    private CancellationTokenSource? _cancellationTokenSource;

    public void Start()
    {
        // Stop the task if it's already running
        Stop();

        _cancellationTokenSource = new CancellationTokenSource();
        Task.Factory.StartNew(() => RunPeriodicAsync(_cancellationTokenSource.Token),
            _cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public void Stop()
    {
        if (_cancellationTokenSource == null) return;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
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