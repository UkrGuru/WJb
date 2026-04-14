namespace WJb.Extensions;

public static class ActionItemFactory
{
    public static ActionItem Create(string type, dynamic? more) 
        => new ActionItem(type, MoreExtensions.ToJsonObject(more));
}