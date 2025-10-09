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

    private int _currentDelay;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _currentDelay = NoDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            await DoWorkAsync(stoppingToken);
            await Task.Delay(_currentDelay, stoppingToken);
        }
    }

    protected virtual async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        int jobId = 0;

        try
        {
            jobId = await StartJobAsync(stoppingToken);

            if (jobId > 0)
            {
                _logger.LogInformation("Processing job with ID: {jobId}", jobId);

                bool exec_result = false, next_result = false;
                try
                {
                    (exec_result, next_result) = await ProcessJobAsync(jobId, stoppingToken);

                    _currentDelay = next_result ? NoDelay : MinDelay;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Job #{JobId} crashed.", jobId);

                    exec_result = false;

                    if (_currentDelay < MaxDelay) _currentDelay += NewDelay;
                }
                finally
                {
                    await FinishJobAsync(jobId, exec_result, stoppingToken);
                }
            }
            else
            {
                _logger.LogInformation("No job to process at: {time}", DateTimeOffset.Now);
                if (_currentDelay < MaxDelay) _currentDelay += NewDelay;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DoWorkAsync");
            if (_currentDelay < MaxDelay) _currentDelay += NewDelay;
        }
    }

    protected virtual Task<int> StartJobAsync(CancellationToken stoppingToken)
    {
        int jobId = _random.Next(0, 2) == 1 ? _random.Next(1, 1000) : 0;

        _logger.LogInformation("StartJobAsync checked. Job ID: {jobId}", jobId);
        return Task.FromResult(jobId);
    }

    protected virtual Task<object> GetJobAsync(int jobId, CancellationToken stoppingToken)
    {
        return Task.FromResult<object>(new { Id = jobId, Name = $"Job-{jobId}" });
    }

    protected virtual async Task<(bool execResult, bool nextResult)> ProcessJobAsync(int jobId, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Executing job logic for Job ID: {jobId}", jobId);

        try
        {
            var job = await GetJobAsync(jobId, stoppingToken); // Simulated job retrieval

            bool execResult = new Random().Next(0, 2) == 1;

            bool nextResult = new Random().Next(0, 2) == 1;

            _logger.LogInformation("Job {JobId} executed: {ExecResult}, next step: {NextResult}", jobId, execResult, nextResult);

            return (execResult, nextResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing job ID: {jobId}", jobId);
            return (false, false); // Safe fallback
        }
    }

    protected virtual Task FinishJobAsync(int jobId, bool exec_result, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Finishing job with ID: {jobId}", jobId);

        return Task.CompletedTask;
    }
}