using Serilog;

namespace worker;

public class Worker : BackgroundService
{

    #region Protected Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Log.Information("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }

    #endregion

}
