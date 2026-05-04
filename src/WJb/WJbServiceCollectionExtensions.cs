// WJb – Base Edition
// Copyright (c) 2025–2026 Oleksandr Viktor (UkrGuru).

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WJb;
using WJb.Extensions;
using WJb.Impl.Base;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service registration helpers for WJb runtime and actions (Base edition only).
/// </summary>
public static class WJbServiceCollectionExtensions
{
    // ------------------------------------------------------------
    // Public entrypoints
    // ------------------------------------------------------------

    public static IServiceCollection AddWJb(
        this IServiceCollection services,
        IDictionary<string, ActionItem>? actions,
        bool addActionFactory = true,
        bool addProcessor = true,
        bool addScheduler = false,
        bool addHostedServices = true)
    {
        return services.AddWJb<JobProcessor>(
            actions: actions,
            addActionFactory: addActionFactory,
            addProcessor: addProcessor,
            addScheduler: addScheduler,
            addHostedServices: addHostedServices);
    }

    public static IServiceCollection AddWJb<TProcessor>(
        this IServiceCollection services,
        IDictionary<string, ActionItem>? actions = null,
        Action<IDictionary<string, ActionItem>>? configureActions = null,
        bool addActionFactory = true,
        bool addProcessor = true,
        bool addScheduler = false,
        bool addHostedServices = true)
        where TProcessor : class, IJobProcessor
    {
        // ------------------------------------------------------------
        // Actions
        // ------------------------------------------------------------

        var actionItems = actions != null
            ? new Dictionary<string, ActionItem>(actions, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase);

        configureActions?.Invoke(actionItems);

        foreach (var kv in actionItems)
        {
            var type = Type.GetType(kv.Value.Type)
                ?? throw new InvalidOperationException(
                    $"Invalid action type: '{kv.Value.Type}'.");

            if (!typeof(IAction).IsAssignableFrom(type))
                throw new InvalidOperationException(
                    $"Type '{type.FullName}' does not implement IAction.");

            services.TryAddTransient(type);
        }

        // ------------------------------------------------------------
        // ActionFactory
        // ------------------------------------------------------------

        if (addActionFactory)
        {
            services.AddSingleton<IActionFactory>(sp =>
                new ActionFactory(sp, actionItems));
        }

        // ------------------------------------------------------------
        // Queue
        // ------------------------------------------------------------

        services.AddSingleton<IJobQueue, InMemoryJobQueue>();

        // ------------------------------------------------------------
        // Processor
        // ------------------------------------------------------------

        if (addProcessor &&
            !services.Any(sd => sd.ServiceType == typeof(IJobProcessor)))
        {
            services.AddSingleton<TProcessor>();
            services.AddSingleton<IJobProcessor>(sp =>
                sp.GetRequiredService<TProcessor>());

            if (addHostedServices &&
                typeof(BackgroundService).IsAssignableFrom(typeof(TProcessor)))
            {
                services.AddHostedService(sp =>
                    (BackgroundService)(object)
                        sp.GetRequiredService<TProcessor>());
            }
        }

        // ------------------------------------------------------------
        // Scheduler
        // ------------------------------------------------------------

        if (addScheduler)
        {
            services.AddSingleton<JobScheduler>();
            services.AddSingleton<IJobScheduler>(sp =>
                sp.GetRequiredService<JobScheduler>());

            if (addHostedServices)
            {
                services.AddHostedService(sp =>
                    sp.GetRequiredService<JobScheduler>());
            }
        }

        return services;
    }
}