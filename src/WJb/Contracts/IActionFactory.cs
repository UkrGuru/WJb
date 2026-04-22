namespace WJb;

/// <summary>
/// Factory for creating actions and accessing action metadata.
/// </summary>
public interface IActionFactory : IActionRegistry
{
    /// <summary>
    /// Creates an action instance by CLR type name.
    /// </summary>
    IAction Create(string actionCode);

    /// <summary>
    /// Returns action metadata by logical action code.
    /// </summary>
    ActionItem GetActionItem(string actionCode);
}

/// <summary>
/// Registry interface for runtime-manageable action configurations.
/// </summary>
public interface IActionRegistry
{
    /// <summary>
    /// Returns a snapshot of the current action configuration.
    /// </summary>
    IReadOnlyDictionary<string, ActionItem> Snapshot();
}
