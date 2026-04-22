using System.Text.Json.Nodes;

namespace WJb;

/// <summary>
/// Represents an action capable of explicitly routing workflow execution
/// by enqueuing the next job.
/// </summary>
public interface IWorkflowAction
{
    /// <summary>
    /// Routes workflow execution to the next action using provided metadata.
    /// </summary>
    Task NextAsync(JsonObject nextMore, CancellationToken stoppingToken = default);

    // Workflow routing is explicit and opt-in.
    // Only actions that implement IWorkflowAction
    // are allowed to enqueue follow-up jobs.
}
