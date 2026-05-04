using System.Text.Json.Nodes;
using WJb.Extensions;

namespace WJb;

/// <summary>
/// Registry for retrieving settings values.
/// Represents an empty/default implementation of <see cref="ISettingsRegistry"/>.
/// </summary>
public sealed class SettingsRegistry : ISettingsRegistry
{
    /// <summary>
    /// An empty settings registry that always returns default values.
    /// </summary>
    public static readonly ISettingsRegistry Empty = new SettingsRegistry();

    private SettingsRegistry() { }

    /// <inheritdoc/>
    public T Get<T>(string key, T defaultValue = default!)
        => defaultValue;
}

