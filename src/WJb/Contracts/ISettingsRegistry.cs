namespace WJb;

/// <summary>
/// Provides access to application settings by key.
/// </summary>
public interface ISettingsRegistry
{
    T Get<T>(string key, T defaultValue = default!);
}
