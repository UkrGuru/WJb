using System.Text.Json;
using System.Text.Json.Nodes;

namespace WJb.Extensions;

/// <summary>
/// Helpers for <see cref="JsonObject"/> / <see cref="JsonNode"/> to make payload handling safe and ergonomic.
/// These helpers are intentionally forgiving: missing or invalid shapes typically return <c>null</c> or defaults
/// rather than throw, so callers can compose behavior without excessive try/catch.
/// </summary>
public static class MoreExtensions
{
    // ----------------------------------------------------------------
    // Simple scalar getters
    // ----------------------------------------------------------------
    public static void AddPriority(this JsonObject element, Priority priority, string propertyName = "priority")
        => element.Add(propertyName, priority.ToString());

    public static Priority GetPriority(this JsonObject? element, string propertyName = "priority", Priority defaultPriority = Priority.Normal)
        => Enum.TryParse<Priority>(element.GetString(propertyName), ignoreCase: true, out var p) ? p : defaultPriority;

    public static string? GetString(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<string?>();

    public static int? GetInt32(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<int?>();

    public static long? GetInt64(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<long?>();

    public static bool? GetBoolean(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<bool?>();

    public static double? GetDouble(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<double?>();

    public static decimal? GetDecimal(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<decimal?>();

    public static DateTime? GetDateTime(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<DateTime?>();

    public static Guid? GetGuid(this JsonObject? element, string propertyName)
        => element?[propertyName]?.GetValue<Guid?>();

    // ----------------------------------------------------------------
    // Nested structures
    // ----------------------------------------------------------------
    /// <summary>Returns a nested <see cref="JsonObject"/> by property name, or <c>null</c> if it's not an object.</summary>
    public static JsonObject? GetObject(this JsonObject? element, string propertyName)
        => element?[propertyName]?.AsObject();

    /// <summary>Returns a <see cref="JsonArray"/> by property name, or <c>null</c> if it's not an array.</summary>
    public static JsonArray? GetArray(this JsonObject? element, string propertyName)
        => element?[propertyName]?.AsArray();

    /// <summary>
    /// Returns an <see cref="IEnumerable{JsonNode}"/> for an array property; empty if missing/not array.
    /// </summary>
    public static IEnumerable<JsonNode?> GetItems(this JsonObject? element, string propertyName)
    {
        var arr = element.GetArray(propertyName);
        if (arr is null || arr.Count == 0) yield break;
        foreach (var item in arr) yield return item;
    }

    // ----------------------------------------------------------------
    // Enum / Typed conversion
    // ----------------------------------------------------------------
    /// <summary>
    /// Parses an enum value from string, case-insensitive; <c>null</c> if missing/invalid.
    /// </summary>
    public static TEnum? GetEnum<TEnum>(this JsonObject? element, string propertyName) where TEnum : struct
    {
        var s = element.GetString(propertyName);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return Enum.TryParse<TEnum>(s, ignoreCase: true, out var value) ? value : (TEnum?)null;
    }

    // ----------------------------------------------------------------
    // Normalization
    // ----------------------------------------------------------------
    /// <summary>
    /// Convert any object (anonymous types, JsonNode, JsonElement, etc.) to <see cref="JsonObject"/>?.
    /// Arrays cannot be coerced into <see cref="JsonObject"/> and will return <c>null</c>.
    /// </summary>
    public static JsonObject? ToJsonObject(dynamic? source)
    {
        if (source is null) return null;

        if (source is JsonObject jo) return jo;

        if (source is JsonNode node)
        {
            if (node is JsonObject obj) return obj;
            return null; // do not coerce arrays into objects
        }

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
            return null;
        }
    }

    // ----------------------------------------------------------------
    // Merge (child-wins overlay semantics)
    // ----------------------------------------------------------------
    /// <summary>
    /// Merge properties from <paramref name="source"/> into <paramref name="target"/>.
    /// - For primitives/values: overwrite.<br/>
    /// - For arrays: deep clone (replace).<br/>
    /// - For objects: recursive merge (child-wins).
    /// </summary>
    public static void MergeInto(JsonObject target, JsonObject? source)
    {
        JsonObject? srcObj = ToJsonObject(source);
        if (srcObj is null) return;

        foreach (var kvp in srcObj)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            switch (value)
            {
                case JsonObject srcNested:
                    if (target[key] is JsonObject dstNested)
                        MergeInto(dstNested, srcNested); // recursive merge
                    else
                        target[key] = srcNested.DeepClone();
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
    // Try-get helpers (avoid exceptions)
    // ----------------------------------------------------------------
    public static bool TryGetString(this JsonObject? element, string propertyName, out string? value)
    {
        value = element.GetString(propertyName);
        return value is not null;
    }

    public static bool TryGetArray(this JsonObject? element, string propertyName, out JsonArray? value)
    {
        value = element.GetArray(propertyName);
        return value is not null;
    }

    public static bool TryGetObject(this JsonObject? element, string propertyName, out JsonObject? value)
    {
        value = element.GetObject(propertyName);
        return value is not null;
    }

    /// <summary>
    /// Extracts all properties from the given <see cref="JsonObject"/> whose keys start with the specified prefix.
    /// The extracted properties are added to a new <see cref="JsonObject"/> with the prefix removed from their keys.
    /// Values are deep-cloned to avoid mutating the original object.
    /// </summary>
    public static JsonObject ExtractPrefixed(this JsonObject? more, string prefix)
    {
        var result = new JsonObject();
        if (more is null) return result;

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

