namespace WJb;

/// <summary>
/// Default implementation of <see cref="IActionFactory"/>.
/// </summary>
public sealed class ActionFactory : IActionFactory
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<string, ActionItem> _actions;

    /// <summary>
    /// Raised when action configuration is reloaded.
    /// </summary>
    public event Action? Reloaded;

    /// <summary>
    /// Creates a new <see cref="ActionFactory"/> instance.
    /// </summary>
    public ActionFactory(IServiceProvider services, IDictionary<string, ActionItem>? actions = default)
    {
        _services = services;
        _actions = new Dictionary<string, ActionItem>(actions 
            ?? new Dictionary<string, ActionItem>(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates an <see cref="IAction"/> by its fully qualified type name.
    /// </summary>
    public IAction Create(string actionType)
    {
        var type = Type.GetType(actionType)
            ?? throw new InvalidOperationException($"Action type '{actionType}' was not found.");

        var action =_services.GetService(type) as IAction
            ?? Activator.CreateInstance(type) as IAction
            ?? throw new InvalidOperationException($"Could not create instance of '{type.FullName}' as IAction.");

        return action;
    }

    /// <summary>
    /// Returns action metadata by action code.
    /// </summary>
    public ActionItem GetActionItem(string actionCode)
        => _actions[actionCode];

    /// <summary>
    /// Reloads action configuration at runtime.
    /// </summary>
    public void Reload(IDictionary<string, ActionItem> newConfig)
    {
        // Available only in the commercial edition.
    }

    /// <summary>
    /// Returns a snapshot of the current action configuration.
    /// </summary>
    public IReadOnlyDictionary<string, ActionItem> Snapshot()
        => _actions;
}