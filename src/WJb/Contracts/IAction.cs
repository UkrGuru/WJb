using System.Text.Json.Nodes;

namespace WJb;

/// <summary>
/// Represents an executable action.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Executes the action with provided metadata.
    /// </summary>
    Task ExecAsync(JsonObject? jobMore, 
        CancellationToken cancellationToken);
}
