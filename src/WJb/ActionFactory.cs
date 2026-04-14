
namespace WJb;

public sealed class ActionFactory(IServiceProvider services, IDictionary<string, ActionItem>? actions = default) : IActionFactory
{
    private readonly IServiceProvider _services = services;
    private readonly Dictionary<string, ActionItem> _actions = new(actions ?? new Dictionary<string, ActionItem>(), StringComparer.OrdinalIgnoreCase);

    public event Action? Reloaded;

    public IAction Create(string actionType)
    {
        var type = Type.GetType(actionType)
            ?? throw new InvalidOperationException($"Action type '{actionType}' was not found.");

        var action = _services.GetService(type) as IAction
            ?? Activator.CreateInstance(type) as IAction
            ?? throw new InvalidOperationException($"Could not create instance of '{type.FullName}' as IAction.");

        return action;
    }

    public ActionItem GetActionItem(string actionCode)
        => _actions[actionCode];

    public void Reload(IDictionary<string, ActionItem> newConfig)
    {
        // Available only in the paid version.
    }

    public IReadOnlyDictionary<string, ActionItem> Snapshot()
        => _actions;
}