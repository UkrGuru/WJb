using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UkrGuru.Sql;
using UkrGuru.WJb.SqlQueries;

namespace UkrGuru.WJb;

public class Scheduler(IConfiguration config, ILogger<Scheduler> logger, IDbService db) : BackgroundService
{
    private readonly IConfiguration _config = config;
    private readonly ILogger<Scheduler> _logger = logger;
    private readonly IDbService _db = db;

    public virtual string AppName => _config["WJbSettings:AppName"] ?? "UnknownApp";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{AppName} Scheduler started.", AppName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CreateJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{AppName} Scheduler.CreateJobsAsync crashed.", AppName);
            }
        }

        _logger.LogInformation("{AppName} Scheduler stopped.", AppName);
    }


    public virtual async Task CreateJobsAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(60000 - DateTime.Now.Second * 1000, stoppingToken);

        await _db.ExecAsync(WJbQueue.Ins_Cron, timeout: 50, cancellationToken: stoppingToken);
    }
}