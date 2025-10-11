using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UkrGuru.Sql;

namespace UkrGuru.WJb;

public class Scheduler(IDbService db, ILogger<Worker> logger) : BackgroundService
{
    private readonly IDbService _db = db;
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextDelay = MinDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            var stepDelay = await DoWorkAsync(stoppingToken);

            await Task.Delay(nextDelay, stoppingToken);
        }
    }

    public virtual async Task<int> DoWorkAsync(CancellationToken stoppingToken)
    {
        var delay = NoDelay;
        return await Task.FromResult(delay);
    }
}
