namespace WJb;

public interface IActionFactory: IReloadableActionRegistry
{
    IAction Create(string actionType);
    ActionItem GetActionItem(string actionCode);
}

public interface IReloadableActionRegistry
{
    IReadOnlyDictionary<string, ActionItem> Snapshot();
    void Reload(IDictionary<string, ActionItem> newConfig);
    event Action? Reloaded;
}