namespace WJb.Extensions;

/// <summary>
/// Factory helpers for creating ActionItem instances.
/// </summary>
public static class ActionItemFactory
{
    /// <summary>
    /// Creates an ActionItem from CLR type name and arbitrary metadata.
    /// </summary>
    public static ActionItem Create(string type, dynamic? more)
        => new ActionItem(type, MoreExtensions.ToJsonObject(more));
}
