using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WJb.Extensions;

/// <summary>
/// Service registration helpers for WJb runtime and actions.
/// </summary>
public static partial class WJbExtensions
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
        Dictionary<string, ActionItem>? finalMap = null;

        if (actions is not null)
            finalMap = new Dictionary<string, ActionItem>(
                actions, StringComparer.OrdinalIgnoreCase);

        if (finalMap is not null && finalMap.Count > 0)
        {
            foreach (var kv in finalMap)
            {
                var typeName = kv.Value.Type;
                var type = Type.GetType(typeName)
                    ?? throw new InvalidOperationException(
                        $"Invalid action type: '{typeName}'. Type could not be resolved.");

                EnsureTransient(services, type);
            }
        }

        if (addActionFactory)
        {
            // Empty action set is valid; actions may be provided later.
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
        if (addScheduler && !addProcessor)
            throw new InvalidOperationException(
                "JobScheduler requires JobProcessor to be enabled.");

        if (addProcessor || addScheduler)
        {
            EnsureSingleton<IJobQueue>(services, sp =>
                new InMemoryJobQueue(
                    sp.GetRequiredService<ILogger<InMemoryJobQueue>>()));
        }

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

    // ----------------------------------------------------------------
    // Manual idempotency helpers
    // ----------------------------------------------------------------

    private static void EnsureSingleton<TService>(
        IServiceCollection services,
        Func<IServiceProvider, object> factory)
    {
        if (!services.Any(d => d.ServiceType == typeof(TService)))
            services.AddSingleton(typeof(TService), sp => factory(sp));
    }

    private static void EnsureTransient(
        IServiceCollection services,
        Type serviceType)
    {
        if (!services.Any(d =>
                d.ServiceType == serviceType &&
                d.Lifetime == ServiceLifetime.Transient))
        {
            services.AddTransient(serviceType);
        }
    }

    private sealed class HostedMarker<THosted>
        where THosted : class, IHostedService
    { }

    private static void EnsureHostedService<THosted>(
        IServiceCollection services)
        where THosted : class, IHostedService
    {
        if (services.Any(d =>
            d.ServiceType == typeof(HostedMarker<THosted>)))
            return;

        services.AddSingleton<HostedMarker<THosted>>(_ =>
            new HostedMarker<THosted>());

        services.AddHostedService(sp =>
            sp.GetRequiredService<THosted>());
    }
}