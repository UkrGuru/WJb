using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UkrGuru.WJb;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private static readonly Random _random = new();

    public virtual int NoDelay => 0;
    public virtual int MinDelay => 100;
    public virtual int NewDelay => 1000;
    public virtual int MaxDelay => 20000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextExecutionDelay = MinDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            var taskSuggestedDelay = await DoWorkAsync(stoppingToken);

            nextExecutionDelay = (taskSuggestedDelay > 0)
                ? Math.Min(nextExecutionDelay + taskSuggestedDelay, MaxDelay)
                : MinDelay;

            await Task.Delay(nextExecutionDelay, stoppingToken);
        }
    }

    public virtual async Task<int> DoWorkAsync(CancellationToken stoppingToken)
    {
        var delay = NoDelay;
        try
        {
            var jobId = await StartJobAsync(stoppingToken);

            if (jobId > 0)
            {
                bool exec_result = false;
                try
                {
                    exec_result = await ProcessJobAsync(jobId, stoppingToken);
                }
                catch 
                {
                    delay = NewDelay;
                }
                finally
                {
                    await FinishJobAsync(jobId, exec_result, stoppingToken);
                }
            }
            else
            {
                delay = NewDelay;
            }
        }
        catch 
        {
            delay = NewDelay;
        }

        return await Task.FromResult(delay);
    }

    public virtual async Task<int> StartJobAsync(CancellationToken stoppingToken)
    {
        int jobId = _random.Next(0, 2) == 1 ? _random.Next(1, 1000) : 0;

        return await Task.FromResult(jobId);
    }

    public virtual async Task<bool> ProcessJobAsync(int jobId, CancellationToken stoppingToken)
    {
        bool execResult = _random.Next(0, 2) == 1;

        return await Task.FromResult(execResult);
    }

    public virtual Task FinishJobAsync(int jobId, bool exec_result, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}