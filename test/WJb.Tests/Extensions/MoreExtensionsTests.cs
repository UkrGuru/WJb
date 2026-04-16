using System.Text.Json;
using System.Text.Json.Nodes;

namespace WJb.Extensions.Tests;

public sealed class MoreExtensionsTests
{
    /* -----------------------------------------------------------
     * Scalar getters
     * -----------------------------------------------------------*/

    [Fact]
    public void GetString_returns_value_when_present()
    {
        var obj = new JsonObject
        {
            ["name"] = "test"
        };

        var value = obj.GetString("name");

        Assert.Equal("test", value);
    }

    [Fact]
    public void GetString_returns_null_when_missing()
    {
        var obj = new JsonObject();

        var value = obj.GetString("missing");

        Assert.Null(value);
    }

    [Fact]
    public void GetInt32_returns_value_when_present()
    {
        var obj = new JsonObject
        {
            ["count"] = 5
        };

        var value = obj.GetInt32("count");

        Assert.Equal(5, value);
    }

    [Fact]
    public void GetBoolean_returns_value_when_present()
    {
        var obj = new JsonObject
        {
            ["enabled"] = true
        };

        var value = obj.GetBoolean("enabled");

        Assert.True(value);
    }

    /* -----------------------------------------------------------
     * Enum helpers
     * -----------------------------------------------------------*/

    private enum TestEnum
    {
        One,
        Two
    }

    [Fact]
    public void GetEnum_parses_case_insensitive()
    {
        var obj = new JsonObject
        {
            ["mode"] = "tWo"
        };

        var value = obj.GetEnum<TestEnum>("mode");

        Assert.Equal(TestEnum.Two, value);
    }

    [Fact]
    public void GetEnum_returns_null_on_invalid_value()
    {
        var obj = new JsonObject
        {
            ["mode"] = "invalid"
        };

        var value = obj.GetEnum<TestEnum>("mode");

        Assert.Null(value);
    }

    /* -----------------------------------------------------------
     * Nested access
     * -----------------------------------------------------------*/

    [Fact]
    public void GetObject_returns_nested_object()
    {
        var nested = new JsonObject
        {
            ["x"] = 1
        };

        var obj = new JsonObject
        {
            ["child"] = nested
        };

        var result = obj.GetObject("child");

        Assert.Same(nested, result);
    }

    [Fact]
    public void GetArray_returns_items_safely()
    {
        var obj = new JsonObject
        {
            ["items"] = new JsonArray(1, 2, 3)
        };

        var items = new List<JsonNode?>();

        foreach (var item in obj.GetItems("items"))
            items.Add(item);

        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void GetItems_returns_empty_for_missing_array()
    {
        var obj = new JsonObject();

        var items = new List<JsonNode?>();

        foreach (var item in obj.GetItems("items"))
            items.Add(item);

        Assert.Empty(items);
    }

    /* -----------------------------------------------------------
     * ToJsonObject normalization
     * -----------------------------------------------------------*/

    [Fact]
    public void ToJsonObject_returns_null_for_null_source()
    {
        var result = MoreExtensions.ToJsonObject(null);

        Assert.Null(result);
    }

    [Fact]
    public void ToJsonObject_returns_same_instance_for_JsonObject()
    {
        var obj = new JsonObject
        {
            ["x"] = 1
        };

        var result = MoreExtensions.ToJsonObject(obj);

        Assert.Same(obj, result);
    }

    [Fact]
    public void ToJsonObject_materializes_JsonElement_object()
    {
        var json = JsonDocument.Parse("{\"a\":1}");
        var element = json.RootElement;

        var result = MoreExtensions.ToJsonObject(element);

        Assert.NotNull(result);
        Assert.Equal(1, result!["a"]!.GetValue<int>());
    }

    [Fact]
    public void ToJsonObject_returns_null_for_array_element()
    {
        var json = JsonDocument.Parse("[1,2,3]");
        var element = json.RootElement;

        var result = MoreExtensions.ToJsonObject(element);

        Assert.Null(result);
    }

    /* -----------------------------------------------------------
     * MergeInto (child-wins semantics)
     * -----------------------------------------------------------*/

    [Fact]
    public void MergeInto_overwrites_primitive_values()
    {
        var target = new JsonObject
        {
            ["x"] = 1
        };

        var source = new JsonObject
        {
            ["x"] = 2
        };

        MoreExtensions.MergeInto(target, source);

        Assert.Equal(2, target["x"]!.GetValue<int>());
    }

    [Fact]
    public void MergeInto_recursively_merges_nested_objects()
    {
        var target = new JsonObject
        {
            ["obj"] = new JsonObject
            {
                ["a"] = 1,
                ["b"] = 2
            }
        };

        var source = new JsonObject
        {
            ["obj"] = new JsonObject
            {
                ["b"] = 99
            }
        };

        MoreExtensions.MergeInto(target, source);

        var nested = target["obj"]!.AsObject();

        Assert.Equal(1, nested["a"]!.GetValue<int>());
        Assert.Equal(99, nested["b"]!.GetValue<int>());
    }

    [Fact]
    public void MergeInto_replaces_arrays_entirely()
    {
        var target = new JsonObject
        {
            ["arr"] = new JsonArray(1, 2)
        };

        var source = new JsonObject
        {
            ["arr"] = new JsonArray(9)
        };

        MoreExtensions.MergeInto(target, source);

        var arr = target["arr"]!.AsArray();

        Assert.Single(arr);
        Assert.Equal(9, arr[0]!.GetValue<int>());
    }

    /* -----------------------------------------------------------
     * TryGet helpers
     * -----------------------------------------------------------*/

    [Fact]
    public void TryGetString_returns_false_when_missing()
    {
        var obj = new JsonObject();

        var ok = obj.TryGetString("x", out var value);

        Assert.False(ok);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetObject_returns_true_when_present()
    {
        var nested = new JsonObject();

        var obj = new JsonObject
        {
            ["child"] = nested
        };

        var ok = obj.TryGetObject("child", out var value);

        Assert.True(ok);
        Assert.Same(nested, value);
    }

    /* -----------------------------------------------------------
     * ExtractPrefixed
     * -----------------------------------------------------------*/

    [Fact]
    public void ExtractPrefixed_strips_prefix_and_copies_values()
    {
        var more = new JsonObject
        {
            ["next_delay"] = 5,
            ["next_priority"] = "high",
            ["fail_delay"] = 99
        };

        var result = more.ExtractPrefixed("next_");

        Assert.Equal(5, result["delay"]!.GetValue<int>());
        Assert.Equal("high", result["priority"]!.GetValue<string>());
        Assert.False(result.ContainsKey("fail_delay"));
    }

    [Fact]
    public void ExtractPrefixed_returns_empty_object_for_null_source()
    {
        var result = ((JsonObject?)null).ExtractPrefixed("x_");

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}