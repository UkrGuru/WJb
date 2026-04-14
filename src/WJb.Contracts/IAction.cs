using System.Text.Json.Nodes;

namespace WJb;

public interface IAction
{
    Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken);
    Task NextAsync(JsonObject nextMore, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
