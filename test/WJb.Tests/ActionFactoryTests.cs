using System.Text.Json.Nodes;

namespace WJb.Tests;

public sealed class ActionFactoryTests
{
    [Fact]
    public void Create_Uses_ServiceProvider_First()
    {
        var action = new TestAction();

        var services = new FakeServiceProvider(new Dictionary<Type, object>
        {
            [typeof(TestAction)] = action
        });

        var factory = new ActionFactory(
            services,
            new Dictionary<string, ActionItem>());

        var result = factory.Create(typeof(TestAction).AssemblyQualifiedName!);

        Assert.Same(action, result);
    }

    [Fact]
    public void Create_Falls_Back_To_Activator()
    {
        var services = new FakeServiceProvider();

        var factory = new ActionFactory(
            services,
            new Dictionary<string, ActionItem>());

        var result = factory.Create(typeof(TestAction).AssemblyQualifiedName!);

        Assert.NotNull(result);
        Assert.IsType<TestAction>(result);
    }

    [Fact]
    public void Create_Unknown_Type_Throws()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            factory.Create("Unknown.Type, Unknown.Assembly"));

        Assert.Contains("was not found", ex.Message);
    }

    [Fact]
    public void Create_Type_Not_IAction_Throws()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            factory.Create(typeof(NotAnAction).AssemblyQualifiedName!));

        Assert.Contains("Could not create instance", ex.Message);
    }

    [Fact]
    public void GetActionItem_Is_Case_Insensitive()
    {
        var item = new ActionItem("test", null);

        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>
            {
                ["MyAction"] = item
            });

        var result = factory.GetActionItem("myaction");

        Assert.Same(item, result);
    }

    [Fact]
    public void GetActionItem_Missing_Key_Throws()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>());

        Assert.Throws<KeyNotFoundException>(() =>
            factory.GetActionItem("missing"));
    }

    [Fact]
    public void Snapshot_Returns_ReadOnly_View()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>
            {
                ["A"] = new ActionItem("a", null)
            });

        var snapshot = factory.Snapshot();

        Assert.Single(snapshot);
        Assert.True(snapshot.ContainsKey("a"));
    }
}

/* =======================
   Test Helpers
   ======================= */

internal sealed class TestAction : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

internal sealed class NotAnAction
{
}

internal sealed class FakeServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services;

    public FakeServiceProvider()
        : this(new Dictionary<Type, object>())
    {
    }

    public FakeServiceProvider(Dictionary<Type, object> services)
    {
        _services = services;
    }

    public object? GetService(Type serviceType)
        => _services.TryGetValue(serviceType, out var service)
            ? service
            : null;
}