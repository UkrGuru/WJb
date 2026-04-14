using System.Text.Json.Nodes;

namespace WJb;

public class ActionItem
{
    public string Type { get; set; } = default!;
    public JsonObject? More { get; set; }

    public ActionItem() { }

    public ActionItem(string type, JsonObject? more)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        More = more;
    }
}