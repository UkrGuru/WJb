using System.Text.Json.Nodes;
using WJb.Actions;

public sealed class WorkflowAction : WorkflowActionBase
{
    protected override Task ExecCoreAsync(JsonObject? jobMore, CancellationToken stoppingToken) 
    {
        // No business logic.
        return Task.CompletedTask;
    }

    protected override async Task ExecNextAsync(bool success, JsonObject jobMore, CancellationToken stoppingToken)
    {
        // routing logic:
        // success / failure / branches
        //...
    }
}
