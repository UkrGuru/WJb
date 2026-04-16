using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WJb.Extensions.Tests;

internal sealed class NullLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    { }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

public sealed class DummyAction
{
}

public sealed class WJbExtensionsTests
{
    /* -----------------------------------------------------------
     * AddWJbActions
     * -----------------------------------------------------------*/

    [Fact]
    public void AddWJbActions_registers_action_type_as_transient()
    {
        var services = new ServiceCollection();

        var actions = new Dictionary<string, ActionItem>
        {
            ["TEST"] = new ActionItem
            {
                Type = typeof(DummyAction).AssemblyQualifiedName!
            }
        };

        services.AddWJbActions(actions);

        var descriptor = Assert.Single(
            services,
            d => d.ServiceType == typeof(DummyAction));

        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddWJbActions_registers_ActionFactory_only_once()
    {
        var services = new ServiceCollection();

        services.AddWJbActions();
        services.AddWJbActions();

        Assert.Single(
            services,
            d => d.ServiceType == typeof(IActionFactory));
    }

    /* -----------------------------------------------------------
     * AddWJbRuntime
     * -----------------------------------------------------------*/

    [Fact]
    public void AddWJbRuntime_does_not_register_queue_when_disabled()
    {
        var services = new ServiceCollection();

        services.AddWJbRuntime(
            addProcessor: false,
            addScheduler: false);

        Assert.DoesNotContain(
            services,
            d => d.ServiceType == typeof(IJobQueue));
    }

    [Fact]
    public void AddWJbRuntime_registers_queue_when_processor_enabled()
    {
        var services = new ServiceCollection();

        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<InMemoryJobQueue>>(
            new NullLogger<InMemoryJobQueue>());

        services.AddWJbActions(addActionFactory: false);
        services.AddWJbRuntime(addProcessor: true);

        Assert.Single(
            services,
            d => d.ServiceType == typeof(IJobQueue));
    }

    [Fact]
    public void AddWJbRuntime_registers_processor_and_interface_alias()
    {
        var services = new ServiceCollection();

        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<JobProcessor>>(
            new NullLogger<JobProcessor>());
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<InMemoryJobQueue>>(
            new NullLogger<InMemoryJobQueue>());

        services.AddWJbActions();
        services.AddWJbRuntime(addProcessor: true);

        Assert.Single(
            services,
            d => d.ServiceType == typeof(JobProcessor));

        Assert.Single(
            services,
            d => d.ServiceType == typeof(IJobProcessor));
    }

    /* -----------------------------------------------------------
     * AddWJb (integration / idempotency)
     * -----------------------------------------------------------*/

    [Fact]
    public void AddWJb_is_idempotent()
    {
        var services = new ServiceCollection();

        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<InMemoryJobQueue>>(
            new NullLogger<InMemoryJobQueue>());
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<JobProcessor>>(
            new NullLogger<JobProcessor>());

        services.AddWJb();
        services.AddWJb();

        Assert.Single(
            services,
            d => d.ServiceType == typeof(IActionFactory));

        Assert.Single(
            services,
            d => d.ServiceType == typeof(IJobQueue));
    }
}