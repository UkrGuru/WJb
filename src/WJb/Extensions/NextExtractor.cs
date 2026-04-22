using System.Text.Json.Nodes;

namespace WJb.Extensions;

/// <summary>
/// Helper for extracting Next/Fail chaining metadata.
/// </summary>
public static class NextExtractor
{
    /// <summary>
    /// Extracts metadata for the next chained job based on execution result.
    /// </summary>
    public static JsonObject? ExtractNextMore(JsonObject? mergedMore, bool success)
    {
        // No metadata — no chaining
        if (mergedMore is null)
            return null;

        // Select branch depending on execution result
        // success  -> "next"
        // failure  -> "fail"
        var branchKey = success ? "next" : "fail";

        // Logical action code for the next job
        var targetCode = mergedMore.GetString(branchKey);
        if (string.IsNullOrWhiteSpace(targetCode))
            return null;

        // Extract prefixed overlay:
        //   next_* or fail_* → forwarded into next job "more"
        var overlay = mergedMore.ExtractPrefixed(success ? "next_" : "fail_");

        // Inject internal control fields used by the pipeline
        overlay?["__branch"] = branchKey;
        overlay?["__code"] = targetCode;
        overlay?["__success"] = success;

        return overlay;
    }
}