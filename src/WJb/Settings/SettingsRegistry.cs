namespace WJb;

public sealed class SettingsRegistry : ISettingsRegistry
{
    public static readonly ISettingsRegistry Empty = new SettingsRegistry();

    private SettingsRegistry() { }

    public T Get<T>(string key, T defaultValue = default!)
        => defaultValue;
}
