using System.Text.Json.Nodes;

namespace WJb.Extensions.Tests;

public sealed class NextExtractorTests
{
    /* -----------------------------------------------------------
     * Guard conditions
     * -----------------------------------------------------------*/

    [Fact]
    public void ExtractNextMore_returns_null_when_more_is_null()
    {
        var result = NextExtractor.ExtractNextMore(
            mergedMore: null,
            success: true);

        Assert.Null(result);
    }

    [Fact]
    public void ExtractNextMore_returns_null_when_target_code_missing()
    {
        var more = new JsonObject
        {
            ["next_timeout"] = 10
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: true);

        Assert.Null(result);
    }

    [Fact]
    public void ExtractNextMore_returns_null_when_target_code_is_whitespace()
    {
        var more = new JsonObject
        {
            ["next"] = "   "
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: true);

        Assert.Null(result);
    }

    /* -----------------------------------------------------------
     * Success branch
     * -----------------------------------------------------------*/

    [Fact]
    public void ExtractNextMore_success_selects_next_branch()
    {
        var more = new JsonObject
        {
            ["next"] = "ACTION_OK"
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: true);

        Assert.NotNull(result);
        Assert.Equal("ACTION_OK", result!["__code"]?.GetValue<string>());
        Assert.True(result["__success"]!.GetValue<bool>());
    }

    [Fact]
    public void ExtractNextMore_success_forwards_next_prefixed_fields()
    {
        var more = new JsonObject
        {
            ["next"] = "ACTION_OK",
            ["next_delay"] = 5,
            ["next_priority"] = "high",
            ["fail_delay"] = 99
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: true)!;

        Assert.Equal(5, result["delay"]!.GetValue<int>());
        Assert.Equal("high", result["priority"]!.GetValue<string>());
        Assert.False(result.ContainsKey("fail_delay"));
    }

    /* -----------------------------------------------------------
     * Failure branch
     * -----------------------------------------------------------*/

    [Fact]
    public void ExtractNextMore_failure_selects_fail_branch()
    {
        var more = new JsonObject
        {
            ["fail"] = "ACTION_FAIL"
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: false);

        Assert.NotNull(result);
        Assert.Equal("ACTION_FAIL", result!["__code"]?.GetValue<string>());
        Assert.False(result["__success"]!.GetValue<bool>());
    }

    [Fact]
    public void ExtractNextMore_failure_forwards_fail_prefixed_fields()
    {
        var more = new JsonObject
        {
            ["fail"] = "ACTION_FAIL",
            ["fail_reason"] = "timeout",
            ["next_reason"] = "ignored"
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: false)!;

        Assert.Equal("timeout", result["reason"]!.GetValue<string>());
        Assert.False(result.ContainsKey("next_reason"));
    }

    /* -----------------------------------------------------------
     * Overlay behavior
     * -----------------------------------------------------------*/

    [Fact]
    public void ExtractNextMore_returns_only_overlay_and_internal_fields()
    {
        var more = new JsonObject
        {
            ["next"] = "ACTION",
            ["next_x"] = 1,
            ["y"] = 2
        };

        var result = NextExtractor.ExtractNextMore(
            mergedMore: more,
            success: true)!;

        Assert.True(result.ContainsKey("x"));
        Assert.True(result.ContainsKey("__code"));
        Assert.True(result.ContainsKey("__success"));
        Assert.False(result.ContainsKey("y"));
    }
}