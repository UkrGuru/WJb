using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UkrGuru.WJb;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private static readonly Random _random = new();

    protected virtual int NoDelay => 0;
    protected virtual int MinDelay => 100;
    protected virtual int NewDelay => 1000;
    protected virtual int MaxDelay => 20000;

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

    protected virtual async Task<int> DoWorkAsync(CancellationToken stoppingToken)
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

    protected virtual Task<int> StartJobAsync(CancellationToken stoppingToken)
    {
        int jobId = _random.Next(0, 2) == 1 ? _random.Next(1, 1000) : 0;

        _logger.LogInformation("StartJobAsync checked. Job ID: {jobId}", jobId);
        return Task.FromResult(jobId);
    }

    protected virtual async Task<bool> ProcessJobAsync(int jobId, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing job logic for Job ID: {jobId}", jobId);

        await Task.CompletedTask;

        try
        {
            //var job = await GetJobAsync(jobId, stoppingToken); // Simulated job retrieval

            bool execResult = new Random().Next(0, 2) == 1;

            bool nextResult = new Random().Next(0, 2) == 1;

            _logger.LogInformation("Job {JobId} executed: {ExecResult}, next step: {NextResult}", jobId, execResult, nextResult);

            return execResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing job ID: {jobId}", jobId);
            return false; // Safe fallback
        }
    }

    protected virtual Task FinishJobAsync(int jobId, bool exec_result, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finishing job with ID: {jobId}", jobId);

        return Task.CompletedTask;
    }
}