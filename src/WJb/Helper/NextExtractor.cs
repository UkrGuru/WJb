
using System.Text.Json.Nodes;

namespace WJb.Extensions;

/// <summary>
/// Builds payload for <c>IAction.NextAsync</c>:
/// <list type="bullet">
/// <item><description><c>__branch</c> ("next" | "fail")</description></item>
/// <item><description><c>__code</c> (target action code)</description></item>
/// <item><description><c>__overlay</c> (child-wins overlay stripped from prefixed keys)</description></item>
/// </list>
/// Returns <c>null</c> if no corresponding branch is configured.
/// </summary>
public static class NextExtractor
{
    public static JsonObject? ExtractNextMore(JsonObject? mergedMore, bool success)
    {
        if (mergedMore is null) return null;

        var branchKey = success ? "next" : "fail";
        var targetCode = mergedMore.GetString(branchKey);
        if (string.IsNullOrWhiteSpace(targetCode)) return null;

        var overlay = mergedMore.ExtractPrefixed(success ? "next_" : "fail_");

        return new JsonObject
        {
            ["__branch"] = branchKey,
            ["__code"] = targetCode,
            ["__overlay"] = overlay
        };
    }
}
