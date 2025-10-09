using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UkrGuru.WJb;

public class Scheduler(ILogger<Scheduler> logger) : BackgroundService
{
    private readonly ILogger<Scheduler> _logger = logger;

    private int _currentDelay;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync(stoppingToken);
            await Task.Delay(_currentDelay, stoppingToken);
        }
    }

    protected virtual async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
        await Task.CompletedTask;
    }
}
