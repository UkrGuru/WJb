using System.Text.Json.Nodes;
using WJb.Extensions;

namespace WJb.Tests;

public sealed class ActionItemTests
{
    [Fact]
    public void Constructor_Sets_Type_And_More()
    {
        var item = new ActionItem("test", new JsonObject { ["A"] = 1 });

        Assert.Equal("test", item.Type);
        Assert.NotNull(item.More);
        Assert.Equal(1, item.More.GetInt32("A"));
    }

    [Fact]
    public void Constructor_Null_Type_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ActionItem(null!, new JsonObject { ["A"] = 1 }));
    }
}