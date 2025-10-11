using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UkrGuru.Sql;
using UkrGuru.WJb.Data;
using UkrGuru.WJb.Extensions;
using UkrGuru.WJb.SqlQueries;

namespace UkrGuru.WJb;

public class Worker(IDbService db, ILogger<Worker> logger) : BackgroundService
{
    private readonly IDbService _db = db;
    private readonly ILogger<Worker> _logger = logger;

    private static readonly Random _random = new();

    public virtual int NoDelay => 0;
    public virtual int MinDelay => 100;
    public virtual int NewDelay => 1000;
    public virtual int MaxDelay => 20000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextDelay = MinDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            var stepDelay = await DoWorkAsync(stoppingToken);

            nextDelay = (stepDelay > 0)
                ? Math.Min(nextDelay + stepDelay, MaxDelay)
                : MinDelay;

            await Task.Delay(nextDelay, stoppingToken);
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
                bool execResult = false;
                try
                {
                    execResult = await ProcessJobAsync(jobId, stoppingToken);
                }
                catch
                {
                    delay = NewDelay;
                }
                finally
                {
                    await FinishJobAsync(jobId, execResult, stoppingToken);
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
        => await _db.ExecAsync<int>(WJbQueue.Start, cancellationToken: stoppingToken);

    public virtual async Task<bool> ProcessJobAsync(int jobId, CancellationToken stoppingToken)
    {
        bool execResult = false, nextResult = false;

        var job = (await _db.ReadAsync<Job>(WJbQueue.Get, jobId, cancellationToken: stoppingToken)).FirstOrDefault();
        ArgumentNullException.ThrowIfNull(job);

        var action = job.CreateAction();

        execResult = await action.ExecuteAsync(stoppingToken);

        nextResult = await action.NextAsync(execResult, stoppingToken);

        return await Task.FromResult(execResult);
    }

    public virtual async Task FinishJobAsync(int jobId, bool execResult, CancellationToken stoppingToken)
        => await _db.ExecAsync(WJbQueue.Finish, new { JobId = jobId, JobStatus = execResult ? JobStatus.Completed : JobStatus.Failed }, cancellationToken: stoppingToken);
}