// WJb – Base Edition
// Copyright (c) 2025–2026 Oleksandr Viktor (UkrGuru).
// Licensed under the WJb Base License.

using System.Collections.ObjectModel;

namespace WJb.Impl.Base;

/// <summary>
/// Creates a new <see cref="ActionFactory"/> instance.
/// </summary>
internal sealed class ActionFactory(
    IServiceProvider services,
    IDictionary<string, ActionItem>? actions = default) : IActionFactory
{
    private readonly IServiceProvider _services = services;
    private readonly Dictionary<string, ActionItem> _actions = new Dictionary<string, ActionItem>(
            actions ?? new Dictionary<string, ActionItem>(),
            StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates an <see cref="IAction"/> by action code.
    /// </summary>
    public IAction Create(string actionCode)
    {
        if (!_actions.TryGetValue(actionCode, out var item))
            throw new InvalidOperationException(
                $"Action with code '{actionCode}' is not registered.");

        var type = Type.GetType(item.Type)
            ?? throw new InvalidOperationException(
                $"Action type '{item.Type}' was not found.");

        if (!typeof(IAction).IsAssignableFrom(type))
            throw new InvalidOperationException(
                $"Type '{type.FullName}' does not implement IAction.");

        var action =
            _services.GetService(type) as IAction
            ?? Activator.CreateInstance(type) as IAction
            ?? throw new InvalidOperationException(
                $"Could not create instance of '{type.FullName}' as IAction.");

        return action;
    }

    /// <summary>
    /// Returns action metadata by action code.
    /// </summary>
    public ActionItem GetActionItem(string actionCode)
        => _actions[actionCode];

    /// <summary>
    /// Returns a snapshot of the current action configuration.
    /// </summary>
    public IReadOnlyDictionary<string, ActionItem> Snapshot()
        => new ReadOnlyDictionary<string, ActionItem>(_actions);
}
