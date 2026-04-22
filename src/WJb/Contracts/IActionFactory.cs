namespace WJb;

/// <summary>
/// Factory for creating actions and accessing action metadata.
/// </summary>
public interface IActionFactory : IActionRegistry
{
    /// <summary>
    /// Expands a job payload into an action code and metadata.
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
