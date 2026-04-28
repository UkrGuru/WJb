namespace WJb.Extensions;

/// <summary>
/// Factory helpers for creating ActionItem instances.
/// </summary>
public static partial class ActionItemFactory
{
    /// <summary>
    /// Creates an ActionItem from CLR type name and arbitrary metadata.
    /// </summary>
    public static ActionItem Create(string type, object? more)
        => new(type, more is null ? null : MoreExtensions.ToJsonObject(more));
}
