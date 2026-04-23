using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WJb.Extensions;

/// <summary>
/// Service registration helpers for WJb runtime and actions.
/// </summary>
public static class WJbExtensions
{
    /// <summary>
    /// Registers WJb services and optional runtime components.
    /// </summary>
    public static IServiceCollection AddWJb(this IServiceCollection services, IDictionary<string, ActionItem>? actions = null,
        bool addActionFactory = true, bool addProcessor = true, bool addScheduler = false, bool addHostedServices = true)
    {
        // Register action metadata and factory
        services.AddWJbActions(actions, addActionFactory);

        // Register runtime components (queue, processor, scheduler)
        services.AddWJbRuntime(addProcessor, addScheduler, addHostedServices);

        return services;
    }

    /// <summary>
    /// Registers WJb actions and optionally the action factory.
    /// </summary>
    public static IServiceCollection AddWJbActions(this IServiceCollection services, IDictionary<string, ActionItem>? actions = null,
        bool addActionFactory = true)
    {
        // Merge action map using case-insensitive action codes
        Dictionary<string, ActionItem>? finalMap = null;
        if (actions is not null)
        {
            finalMap = new Dictionary<string, ActionItem>(actions, StringComparer.OrdinalIgnoreCase);
        }

        // Register each action CLR type as transient
        // (actions are expected to be stateless)
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

        // Register ActionFactory as singleton
        // Factory holds action metadata snapshot
        if (addActionFactory)
        {
            EnsureSingleton<IActionFactory>(services, sp =>
                finalMap is not null && finalMap.Count > 0
                    ? new ActionFactory(sp, finalMap)
                    : new ActionFactory(sp));
        }

        return services;
    }

    /// <summary>
    /// Registers WJb runtime components such as queue, processor, scheduler, and hosted services.
    /// </summary>
    public static IServiceCollection AddWJbRuntime(this IServiceCollection services,
        bool addProcessor = true, bool addScheduler = false, bool addHostedServices = true)
    {
        // Queue is required if either processor or scheduler is enabled
        if (addProcessor || addScheduler)
        {
            EnsureSingleton<IJobQueue>(services, sp =>
                new InMemoryJobQueue(sp.GetRequiredService<ILogger<InMemoryJobQueue>>()));
        }

        // Processor: singleton implementation + interface alias + optional hosted service
        if (addProcessor)
        {
            EnsureSingleton<JobProcessor>(services, sp => new JobProcessor(
                sp.GetRequiredService<IJobQueue>(),
                sp.GetRequiredService<IActionFactory>(),
                sp.GetRequiredService<ILogger<JobProcessor>>()));

            EnsureSingleton<IJobProcessor>(services, sp =>
                sp.GetRequiredService<JobProcessor>());

            if (addHostedServices)
                EnsureHostedService<JobProcessor>(services);
        }

        // Scheduler: singleton + optional hosted service
        if (addScheduler)
        {
            EnsureSingleton<JobScheduler>(services, sp => new JobScheduler(
                sp.GetRequiredService<IJobQueue>(),
                sp.GetRequiredService<IActionFactory>(),
                sp.GetRequiredService<IJobProcessor>(),
                sp.GetRequiredService<ILogger<JobScheduler>>()));

            if (addHostedServices)
                EnsureHostedService<JobScheduler>(services);
        }

        return services;
    }

    // ─────────────────────────────────────────────────────────────
    // Manual idempotency helpers (no TryAdd*)
    // ─────────────────────────────────────────────────────────────

    private static void EnsureSingleton<TService>(IServiceCollection services, Func<IServiceProvider, object> factory)
    {
        // Register only if the service type is not already present
        if (!services.Any(d => d.ServiceType == typeof(TService)))
        {
            services.AddSingleton(typeof(TService), sp => factory(sp));
        }
    }

    private static void EnsureTransient(IServiceCollection services, Type serviceType)
    {
        // Avoid duplicate transient registrations for the same CLR type
        if (!services.Any(d => d.ServiceType == serviceType && d.Lifetime == ServiceLifetime.Transient))
        {
            services.AddTransient(serviceType);
        }
    }

    // Marker type to ensure hosted service is registered only once per THosted
    private sealed class HostedMarker<THosted> where THosted : class, IHostedService { }

    private static void EnsureHostedService<THosted>(IServiceCollection services) where THosted : class, IHostedService
    {
        // If marker exists, hosted service is already registered
        if (services.Any(d => d.ServiceType == typeof(HostedMarker<THosted>)))
            return;

        // Register marker and reuse the same singleton instance
        services.AddSingleton<HostedMarker<THosted>>(_ => new HostedMarker<THosted>());
        services.AddHostedService(sp => sp.GetRequiredService<THosted>());
    }
}