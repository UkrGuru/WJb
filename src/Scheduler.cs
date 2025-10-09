using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UkrGuru.WJb;

public class Scheduler(ILogger<Scheduler> logger) : BackgroundService
{
    private readonly ILogger<Scheduler> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Scheduler running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
