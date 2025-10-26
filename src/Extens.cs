// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UkrGuru.Sql;
using UkrGuru.WJb.SqlQueries;

namespace UkrGuru.WJb;

public static class Extens
{
    public static IServiceCollection AddWJb(this IServiceCollection services, string? connectionString = null, int nThreads = 4, bool initDb = false)
    {
        return services.AddSql(connectionString, singleton: true)
            .AddWJb(nThreads, initDb);
    }

    public static IServiceCollection AddWJb(this IServiceCollection services, int nThreads = 0, bool initDb = false)
    {
        if (initDb) Assembly.GetExecutingAssembly().InitDb();

        if (nThreads > 0)
        {
            try { DbHelper.Exec(WJbQueue.Finish_All); } catch { }

            services.AddHostedService<Scheduler>();

            //for (int i = 0; i < nThreads; i++)
            //    services.AddSingleton<IHostedService, Worker>();
        }

        return services;
    }

    public static bool InitDb(this Assembly? assembly, string resourceName = "InitDb.sql")
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        string name = assembly.GetName().Name ?? throw new InvalidOperationException("Assembly name is null.");
        string version = assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        string currentDbVersion = DbHelper.Exec<string>(Sys.Get_Ver, name) ?? "0.0.0.0";

        if (currentDbVersion.CompareTo(version) != 0)
        {
            assembly.ExecResource($"{name}.Resources.{resourceName}");
            DbHelper.Exec(Sys.Upd_Ver, new { Name = name, Value = version });
        }

        return true;
    }

    public static void ExecResource(this Assembly assembly, string resourceName, int? timeout = null)
    {
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using StreamReader streamReader = new(stream);
        DbHelper.Exec(streamReader.ReadToEnd(), null, timeout);
    }
}
