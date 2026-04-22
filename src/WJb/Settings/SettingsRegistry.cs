namespace WJb;

/// <summary>
/// Default settings registry that always returns the provided default value.
/// </summary>
public sealed class SettingsRegistry : ISettingsRegistry
{
    public static readonly ISettingsRegistry Empty = new SettingsRegistry();

    private SettingsRegistry() { }

    public T Get<T>(string key, T defaultValue = default!)
        => defaultValue;
}
