// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

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

    public virtual string AppName => _config["WJbSettings:AppName"] ?? "WJbApp";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{AppName} Scheduler started.", AppName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(DateTime.Now, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{AppName} Scheduler.DoWorkAsync crashed.", AppName);
            }
        }

        _logger.LogInformation("{AppName} Scheduler stopped.", AppName);
    }

    public virtual async Task DoWorkAsync(DateTime now, CancellationToken stoppingToken)
    {
        int millisecondsDelay = CalcDelay(now);

        await Task.Delay(millisecondsDelay, stoppingToken);

        await CreateJobsAsync(stoppingToken);
    }

    public virtual int CalcDelay(DateTime now) => 60000 - now.Second * 1000 - now.Millisecond + 25;

    public virtual async Task CreateJobsAsync(CancellationToken stoppingToken)
        => await _db.ExecAsync(WJbQueue.Ins_Cron, timeout: 50, cancellationToken: stoppingToken);
}