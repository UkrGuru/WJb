using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

namespace WJb.Tests;

public sealed class MoreExtensions_PublicTests
{
    [Fact]
    public void GetString_Works_For_String_And_Number()
    {
        var obj = new JsonObject
        {
            ["a"] = "hello",
            ["b"] = 123
        };

        Assert.Equal("hello", obj.GetString("a"));
        Assert.Equal("123", obj.GetString("b"));
        Assert.Null(obj.GetString("missing"));
    }

    [Fact]
    public void GetEnum_Returns_Value_Or_Null()
    {
        var obj = new JsonObject { ["p"] = "High" };

        Assert.Equal(Priority.High, obj.GetEnum<Priority>("p"));
        Assert.Null(obj.GetEnum<Priority>("missing"));
    }

    [Fact]
    public void MergeInto_Child_Wins()
    {
        var target = new JsonObject
        {
            ["a"] = 1,
            ["b"] = new JsonObject { ["x"] = 1 }
        };

        var source = new JsonObject
        {
            ["b"] = new JsonObject { ["x"] = 2 }
        };

        MoreExtensions.MergeInto(target, source);

        Assert.Equal("2", target["b"]!["x"]!.ToString());
    }
}
