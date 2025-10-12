using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UkrGuru.Sql;
using UkrGuru.WJb.Actions;
using UkrGuru.WJb.Data;
using UkrGuru.WJb.SqlQueries;

namespace UkrGuru.WJb;

public class Worker(IConfiguration config, ILogger<Worker> logger, IDbService db) : BackgroundService
{
    private readonly IConfiguration _config = config;
    private readonly ILogger<Worker> _logger = logger;
    private readonly IDbService _db = db;

    public virtual string AppName => _config["WJbSettings:AppName"] ?? "UnknownApp";

    public virtual int NoDelay => 0;
    public virtual int MinDelay => 100;
    public virtual int NewDelay => 1000;
    public virtual int MaxDelay => 20000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{AppName} Worker started.", AppName);

        var nextDelay = MinDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            var stepDelay = await DoWorkAsync(stoppingToken);

            nextDelay = (stepDelay > 0)
                ? Math.Min(nextDelay + stepDelay, MaxDelay)
                : MinDelay;

            await Task.Delay(nextDelay, stoppingToken);
        }

        _logger.LogInformation("{AppName} Worker stopped.", AppName);
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
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{AppName} Job #{JobId} crashed.", AppName, jobId);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "{AppName} Worker.DoWorkAsync crashed.", AppName);

            delay = NewDelay;
        }

        return await Task.FromResult(delay);
    }

    public virtual async Task<int> StartJobAsync(CancellationToken stoppingToken)
        => await _db.ExecAsync<int>(WJbQueue.Start, cancellationToken: stoppingToken);

    public virtual async Task<bool> ProcessJobAsync(int jobId, CancellationToken stoppingToken)
    {
        bool execResult = false, nextResult = false;

        var job = await GetJobAsync(jobId, stoppingToken);
        ArgumentNullException.ThrowIfNull(job);

        var action = CreateAction(job);

        execResult = await action.ExecAsync(stoppingToken);

        nextResult = await action.NextAsync(execResult, stoppingToken);

        return await Task.FromResult(execResult);
    }

    public virtual async Task FinishJobAsync(int jobId, bool execResult, CancellationToken stoppingToken)
        => await _db.ExecAsync(WJbQueue.Finish, new { JobId = jobId, JobStatus = execResult ? JobStatus.Completed : JobStatus.Failed }, cancellationToken: stoppingToken);

    public virtual async Task<Job?> GetJobAsync(int jobId, CancellationToken stoppingToken)
        => (await _db.ReadAsync<Job>(WJbQueue.Get, jobId, cancellationToken: stoppingToken)).FirstOrDefault();

    public virtual IAction CreateAction(Job job)
    {
        ArgumentNullException.ThrowIfNull(job.ActionType);

        var type = Type.GetType($"UkrGuru.WJb.Actions.{job.ActionType}") ?? Type.GetType(job.ActionType);
        ArgumentNullException.ThrowIfNull(type);

        var action = Activator.CreateInstance(type) as IAction;
        ArgumentNullException.ThrowIfNull(action);

        action.Init(job);

        return action;
    }
}