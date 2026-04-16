using System.Text.Json;
using System.Text.Json.Nodes;

namespace WJb.Extensions;

/// <summary>
/// Helpers for JsonObject / JsonNode to make payload handling safe and ergonomic.
/// </summary>
public static class MoreExtensions
{
    // ----------------------------------------------------------------
    // Simple scalar getters
    // ----------------------------------------------------------------

    public static void AddPriority(this JsonObject element, Priority priority, string propertyName = "priority")
        => element.Add(propertyName, priority.ToString());

    public static Priority GetPriority(this JsonObject? element, string propertyName = "priority", Priority defaultPriority = Priority.Normal)
        // Parse enum value safely; fallback to default on missing/invalid value
        => Enum.TryParse<Priority>(element.GetString(propertyName), ignoreCase: true, out var p)
            ? p : defaultPriority;

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

    public static JsonObject? GetObject(this JsonObject? element, string propertyName)
        // Safe cast: returns null if node is not an object
        => element?[propertyName]?.AsObject();

    public static JsonArray? GetArray(this JsonObject? element, string propertyName)
        // Safe cast: returns null if node is not an array
        => element?[propertyName]?.AsArray();

    public static IEnumerable<JsonNode?> GetItems(this JsonObject? element, string propertyName)
    {
        var arr = element.GetArray(propertyName);

        // Treat missing or empty arrays as empty sequence
        if (arr is null || arr.Count == 0)
            yield break;

        foreach (var item in arr)
            yield return item;
    }

    // ----------------------------------------------------------------
    // Enum / typed conversion
    // ----------------------------------------------------------------

    public static TEnum? GetEnum<TEnum>(this JsonObject? element, string propertyName) where TEnum : struct
    {
        var s = element.GetString(propertyName);
        if (string.IsNullOrWhiteSpace(s))
            return null;

        // Case-insensitive enum parsing
        return Enum.TryParse<TEnum>(s, ignoreCase: true, out var value)
            ? value
            : (TEnum?)null;
    }

    // ----------------------------------------------------------------
    // Normalization
    // ----------------------------------------------------------------

    public static JsonObject? ToJsonObject(dynamic? source)
    {
        if (source is null)
            return null;

        // Fast-path for already normalized types
        if (source is JsonObject jo)
            return jo;

        if (source is JsonNode node)
        {
            // Explicitly reject arrays
            return node is JsonObject obj ? obj : null;
        }

        if (source is JsonElement el)
        {
            // Materialize JsonElement.object into JsonNode
            if (el.ValueKind == JsonValueKind.Object)
                return JsonNode.Parse(el.GetRawText()) as JsonObject;

            return null;
        }

        try
        {
            // Fallback: serialize arbitrary objects and reparse
            var json = JsonSerializer.Serialize(source);
            return JsonNode.Parse(json) as JsonObject;
        }
        catch
        {
            // Intentionally forgiving: never throw here
            return null;
        }
    }

    // ----------------------------------------------------------------
    // Merge (child-wins overlay semantics)
    // ----------------------------------------------------------------

    public static void MergeInto(JsonObject target, JsonObject? source)
    {
        JsonObject? srcObj = ToJsonObject(source);
        if (srcObj is null)
            return;

        foreach (var kvp in srcObj)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            switch (value)
            {
                case JsonObject srcNested:
                    // Recursive merge for nested objects
                    if (target[key] is JsonObject dstNested)
                        MergeInto(dstNested, srcNested);
                    else
                        target[key] = srcNested.DeepClone();
                    break;

                case JsonArray srcArr:
                    // Arrays are replaced entirely
                    target[key] = srcArr.DeepClone();
                    break;

                default:
                    // Primitive/value overwrite
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

    public static JsonObject ExtractPrefixed(this JsonObject? more, string prefix)
    {
        var result = new JsonObject();

        if (more is null)
            return result;

        // Copy all prefixed properties, stripping the prefix from keys
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