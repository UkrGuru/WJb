using System.Text.Json.Nodes;

namespace WJb;

public interface IAction
{
    Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken);
}
