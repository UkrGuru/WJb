namespace WJb;

/// <summary>
/// Factory for creating actions and accessing action metadata.
/// </summary>
public interface IActionFactory : IReloadableActionRegistry
{
    /// <summary>
    /// Creates an action instance by CLR type name.
    /// </summary>
    IAction Create(string actionType);

    /// <summary>
    /// Returns action metadata by logical action code.
    /// </summary>
    ActionItem GetActionItem(string actionCode);
}

/// <summary>
/// Registry interface for runtime-manageable action configurations.
/// </summary>
public interface IReloadableActionRegistry
{
    /// <summary>
    /// Returns a snapshot of the current action configuration.
    /// </summary>
    IReadOnlyDictionary<string, ActionItem> Snapshot();

    /// <summary>
    /// Replaces the current action configuration.
    /// </summary>
    void Reload(IDictionary<string, ActionItem> newConfig);

    /// <summary>
    /// Raised when the configuration is reloaded.
    /// </summary>
    event Action? Reloaded;
}