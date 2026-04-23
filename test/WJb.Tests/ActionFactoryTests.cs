using System.Text.Json.Nodes;

namespace WJb.Tests;

public sealed class ActionFactoryTests
{
    [Fact]
    public void Create_Uses_ServiceProvider_First()
    {
        var action = new TestAction();

        var services = new FakeServiceProvider(new()
        {
            [typeof(TestAction)] = action
        });

        var factory = new ActionFactory(
            services,
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
            {
                ["test"] = new ActionItem(
                    typeof(TestAction).AssemblyQualifiedName!,
                    more: null)
            });

        var result = factory.Create("test");

        Assert.Same(action, result);
    }

    [Fact]
    public void Create_Falls_Back_To_Activator()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
            {
                ["test"] = new ActionItem(
                    typeof(TestAction).AssemblyQualifiedName!,
                    more: null)
            });

        var result = factory.Create("test");

        Assert.NotNull(result);
        Assert.IsType<TestAction>(result);
    }

    [Fact]
    public void Create_Unknown_ActionCode_Throws_InvalidOperationException()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase));

        Assert.Throws<InvalidOperationException>(() =>
            factory.Create("missing"));
    }

    [Fact]
    public void Create_Type_Not_IAction_Throws()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
            {
                ["bad"] = new ActionItem(
                    typeof(NotAnAction).AssemblyQualifiedName!,
                    more: null)
            });

        Assert.Throws<InvalidOperationException>(() =>
            factory.Create("bad"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Empty_ActionCode_Treated_As_Not_Registered(string actionCode)
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase));

        Assert.Throws<InvalidOperationException>(() =>
            factory.Create(actionCode));
    }

    [Fact]
    public void Create_Null_ActionCode_Throws_ArgumentNullException()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase));

        Assert.Throws<ArgumentNullException>(() =>
            factory.Create(null!));
    }

    [Fact]
    public void GetActionItem_Is_Case_Insensitive()
    {
        var item = new ActionItem("x", null);

        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
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
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase));

        Assert.Throws<KeyNotFoundException>(() =>
            factory.GetActionItem("missing"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetActionItem_Empty_Key_Throws_KeyNotFound(string actionCode)
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase));

        Assert.Throws<KeyNotFoundException>(() =>
            factory.GetActionItem(actionCode));
    }

    [Fact]
    public void GetActionItem_Null_Key_Throws_ArgumentNullException()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase));

        Assert.Throws<ArgumentNullException>(() =>
            factory.GetActionItem(null!));
    }

    [Fact]
    public void Snapshot_Returns_Case_Insensitive_View()
    {
        var factory = new ActionFactory(
            new FakeServiceProvider(),
            new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
            {
                ["A"] = new ActionItem("a", null)
            });

        var snapshot = factory.Snapshot();

        Assert.Single(snapshot);
        Assert.True(snapshot.ContainsKey("a"));
    }
}

/* =======================
   Test helpers
   ======================= */

internal sealed class TestAction : IAction
{
    public Task ExecAsync(
        JsonObject? jobMore,
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}

internal sealed class NotAnAction
{
}

internal sealed class FakeServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services;

    public FakeServiceProvider()
        : this(new())
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