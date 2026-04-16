using System.Text.Json.Nodes;

namespace WJb;

/// <summary>
/// Describes an action definition and its default metadata.
/// </summary>
public class ActionItem
{
    /// <summary>
    /// Fully qualified CLR type name of the action.
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Default metadata applied to the action.
    /// </summary>
    public JsonObject? More { get; set; }

    public ActionItem() { }

    public ActionItem(string type, JsonObject? more)
    {
        // Type name is mandatory for action resolution
        Type = type ?? throw new ArgumentNullException(nameof(type));

        // Metadata may be null and merged later at runtime
        More = more;
    }
}