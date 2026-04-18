namespace WJb;

public interface ISettingsRegistry
{
    T Get<T>(string key, T defaultValue = default!);
}