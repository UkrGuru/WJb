using System.Text.Json;
using System.Text.Json.Nodes;

namespace WJb.Extensions;

/// <summary>
/// Helpers for JsonObject / JsonNode to make metadata handling safe and ergonomic.
/// Free edition: liberal parsing, no strict typing.
/// </summary>
public static partial class MoreExtensions
{
    // ----------------------------------------------------------------
    // Scalars (liberal, non-throwing)
    // ----------------------------------------------------------------

    /// <summary>
    /// Safely gets a value as string.
    /// Works with strings, numbers, booleans and null.
    /// Never throws.
    /// </summary>
    public static string? GetString(this JsonObject? element, string propertyName)
    {
        if (element is null)
            return null;

        var node = element[propertyName];
        if (node is null)
            return null;

        return node.ToString();
    }

    /// <summary>
    /// Gets an enum value from a string-based metadata entry.
    /// Returns null if missing or invalid.
    /// </summary>
    public static TEnum? GetEnum<TEnum>(
        this JsonObject? element,
        string propertyName)
        where TEnum : struct
    {
        var s = element.GetString(propertyName);
        if (string.IsNullOrWhiteSpace(s))
            return null;

        return Enum.TryParse<TEnum>(s, ignoreCase: true, out var value)
            ? value
            : (TEnum?)null;
    }

    /// <summary>
    /// Gets priority from metadata, falling back to default.
    /// </summary>
    public static Priority GetPriority(
        this JsonObject? element,
        string propertyName = "priority",
        Priority defaultPriority = Priority.Normal)
        => element.GetEnum<Priority>(propertyName) ?? defaultPriority;

    /// <summary>
    /// Adds priority as a string enum value.
    /// </summary>
    public static void AddPriority(
        this JsonObject element,
        Priority priority,
        string propertyName = "priority")
        => element[propertyName] = priority.ToString();

    // ----------------------------------------------------------------
    // Objects / arrays
    // ----------------------------------------------------------------

    public static JsonObject? GetObject(this JsonObject? element, string propertyName)
        => element?[propertyName]?.AsObject();

    public static JsonArray? GetArray(this JsonObject? element, string propertyName)
        => element?[propertyName]?.AsArray();

    public static IEnumerable<JsonNode?> GetItems(this JsonObject? element, string propertyName)
    {
        var arr = element.GetArray(propertyName);
        if (arr is null || arr.Count == 0)
            yield break;

        foreach (var item in arr)
            yield return item;
    }

    // ----------------------------------------------------------------
    // Normalization
    // ----------------------------------------------------------------

    /// <summary>
    /// Normalizes arbitrary input to JsonObject.
    /// Never throws.
    /// Returns null if conversion is impossible.
    /// </summary>
    public static JsonObject? ToJsonObject(object? source)
    {
        if (source is null)
            return null;

        if (source is JsonObject jo)
            return jo;

        if (source is JsonNode node)
            return node as JsonObject;

        if (source is JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Object)
                return JsonNode.Parse(el.GetRawText()) as JsonObject;

            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(source);
            return JsonNode.Parse(json) as JsonObject;
        }
        catch
        {
            // Intentionally forgiving
            return null;
        }
    }

    // ----------------------------------------------------------------
    // Merge (child-wins overlay)
    // ----------------------------------------------------------------

    public static void MergeInto(JsonObject target, JsonObject? source)
    {
        var src = ToJsonObject(source);
        if (src is null)
            return;

        foreach (var kvp in src)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            switch (value)
            {
                case JsonObject srcObj:
                    if (target[key] is JsonObject dstObj)
                        MergeInto(dstObj, srcObj);
                    else
                        target[key] = srcObj.DeepClone();
                    break;

                case JsonArray srcArr:
                    target[key] = srcArr.DeepClone();
                    break;

                default:
                    target[key] = value?.DeepClone();
                    break;
            }
        }
    }

    // ----------------------------------------------------------------
    // Try‑get helpers
    // ----------------------------------------------------------------

    public static bool TryGetString(this JsonObject? element, string propertyName, out string? value)
    {
        value = element.GetString(propertyName);
        return value is not null;
    }

    public static bool TryGetObject(this JsonObject? element, string propertyName, out JsonObject? value)
    {
        value = element.GetObject(propertyName);
        return value is not null;
    }

    public static bool TryGetArray(this JsonObject? element, string propertyName, out JsonArray? value)
    {
        value = element.GetArray(propertyName);
        return value is not null;
    }

    // ----------------------------------------------------------------
    // Utility
    // ----------------------------------------------------------------

    /// <summary>
    /// Extracts all properties with the given prefix into a new object,
    /// stripping the prefix from keys.
    /// </summary>
    public static JsonObject ExtractPrefixed(this JsonObject? more, string prefix)
    {
        var result = new JsonObject();

        if (more is null)
            return result;

        foreach (var kvp in more)
        {
            if (kvp.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                var stripped = kvp.Key[prefix.Length..];
                result[stripped] = kvp.Value?.DeepClone();
            }
        }

        return result;
    }
}
