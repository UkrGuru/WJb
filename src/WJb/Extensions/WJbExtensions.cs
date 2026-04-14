
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WJb.Extensions;

public static class WJbExtensions
{
    public static IServiceCollection AddWJb(
        this IServiceCollection services,
        IDictionary<string, ActionItem>? actions = null,
        bool addActionFactory = true,
        bool addProcessor = true,
        bool addScheduler = false,
        bool addHostedServices = true)
    {
        services.AddWJbActions(actions, addActionFactory);
        services.AddWJbRuntime(addProcessor, addScheduler, addHostedServices);
        return services;
    }

    public static IServiceCollection AddWJbActions(
        this IServiceCollection services,
        IDictionary<string, ActionItem>? actions = null,
        bool addActionFactory = true)
    {
        // Merge action map
        Dictionary<string, ActionItem>? finalMap = null;

        if (actions is not null)
        {
            finalMap = new Dictionary<string, ActionItem>(
                actions,
                StringComparer.OrdinalIgnoreCase);
        }

        // Register each action CLR type as transient
        if (finalMap is not null && finalMap.Count > 0)
        {
            foreach (var kv in finalMap)
            {
                var typeName = kv.Value.Type;

                var type = Type.GetType(typeName)
                    ?? throw new Exception($"Invalid action type: '{typeName}'");

                EnsureTransient(services, type);
            }
        }

        // Register ActionFactory
        if (addActionFactory)
        {
            EnsureSingleton<IActionFactory>(services, sp =>
                finalMap is not null && finalMap.Count > 0
                    ? new ActionFactory(sp, finalMap)
                    : new ActionFactory(sp));
        }

        return services;
    }

    public static IServiceCollection AddWJbRuntime(
        this IServiceCollection services,
        bool addProcessor = true,
        bool addScheduler = false,
        bool addHostedServices = true)
    {
        // Queue needed if processor or scheduler is on
        if (addProcessor || addScheduler)
        {
            // InMemoryJobQueue(IOptions<Dictionary<string, object>>, ILogger<InMemoryJobQueue>)
            EnsureSingleton<IJobQueue>(services, sp =>
                new InMemoryJobQueue(sp.GetRequiredService<ILogger<InMemoryJobQueue>>()));
        }

        // Processor: singleton + alias + (optional) hosted
        if (addProcessor)
        {
            // JobProcessor(IJobQueue, IActionFactory, IReloadableSettingsRegistry, ILogger<JobProcessor>)
            EnsureSingleton<JobProcessor>(services, sp => new JobProcessor(
                sp.GetRequiredService<IJobQueue>(),
                sp.GetRequiredService<IActionFactory>(),
                sp.GetRequiredService<ILogger<JobProcessor>>()));

            EnsureSingleton<IJobProcessor>(services, sp =>
                sp.GetRequiredService<JobProcessor>());

            if (addHostedServices)
                EnsureHostedService<JobProcessor>(services);
        }

        // Scheduler: singleton + (optional) hosted
        if (addScheduler)
        {
            //// JobScheduler(IJobQueue, IActionFactory, IJobProcessor, ILogger<JobScheduler>)
            //EnsureSingleton<JobScheduler>(services, sp => new JobScheduler(
            //    sp.GetRequiredService<IJobQueue>(),
            //    sp.GetRequiredService<IActionFactory>(),
            //    sp.GetRequiredService<IJobProcessor>(),
            //    sp.GetRequiredService<ILogger<JobScheduler>>()));

            //if (addHostedServices)
            //    EnsureHostedService<JobScheduler>(services);
        }

        return services;
    }

    // ─────────────────────────────────────────────────────────────
    // Manual idempotency helpers (no TryAdd*)
    // ─────────────────────────────────────────────────────────────
    private static void EnsureSingleton<TService>(IServiceCollection services, Func<IServiceProvider, object> factory)
    {
        if (!services.Any(d => d.ServiceType == typeof(TService)))
        {
            services.AddSingleton(typeof(TService), sp => factory(sp));
        }
    }

    private static void EnsureTransient(IServiceCollection services, Type serviceType)
    {
        if (!services.Any(d => d.ServiceType == serviceType && d.Lifetime == ServiceLifetime.Transient))
        {
            services.AddTransient(serviceType);
        }
    }

    // Marker type to ensure we only register a hosted service once per THosted.
    private sealed class HostedMarker<THosted> where THosted : class, IHostedService { }

    private static void EnsureHostedService<THosted>(IServiceCollection services)
        where THosted : class, IHostedService
    {
        // If the marker exists, we've already registered the hosted service.
        if (services.Any(d => d.ServiceType == typeof(HostedMarker<THosted>)))
            return;

        // Add the marker and the hosted service factory reusing the same singleton instance.
        services.AddSingleton<HostedMarker<THosted>>(_ => new HostedMarker<THosted>());
        services.AddHostedService(sp => sp.GetRequiredService<THosted>());
    }
}

