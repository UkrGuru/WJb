using System.Text.Json.Nodes;

namespace WJb.Actions;

public sealed class WorkflowAction : WorkflowActionBase
{
    protected override Task ExecCoreAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken)
    {
        // No business logic.
        return Task.CompletedTask;
    }

    protected override Task ExecNextAsync(
        bool success,
        JsonObject jobMore,
        CancellationToken stoppingToken)
    {
        // routing logic:
        // success / failure / branches
        // jobMore is always non-null and safe to mutate
        return Task.CompletedTask;
    }
}

// Invariant:
// - Actions control their own execution lifecycle.
// - Workflow routing is explicit and action-owned.
// - JobProcessor executes actions but never orchestrates them.

public abstract class WorkflowActionBase : IAction
{
    protected abstract Task ExecCoreAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken);

    /// <summary>
    /// Executes workflow routing after core execution.
    /// jobMore is a deep clone of original metadata and may be mutated.
    /// </summary>
    protected abstract Task ExecNextAsync(
        bool success,
        JsonObject jobMore,
        CancellationToken stoppingToken);

    public async Task ExecAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken)
    {
        // Invariants:
        // - Core is executed exactly once
        // - Routing is executed exactly once
        // - Execution result is explicit

        bool success = false;
        Exception? coreException = null;
        Exception? routingException = null;

        // Phase 1: Core execution
        try
        {
            await ExecCoreAsync(jobMore, stoppingToken)
                .ConfigureAwait(false);

            success = true;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Cancellation is always authoritative
            throw;
        }
        catch (Exception ex)
        {
            coreException = ex;
            success = false;
        }

        // Phase 2: Routing (always executed)
        try
        {
            var nextMore = jobMore?.DeepClone() as JsonObject ?? new JsonObject();

            await ExecNextAsync(success, nextMore, stoppingToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Routing cancellation only matters
            // if core did not already fail
            if (coreException is null)
                routingException = new OperationCanceledException(stoppingToken);
        }
        catch (Exception ex)
        {
            // Routing exceptions only propagate if core succeeded
            if (success)
            {
                routingException = new InvalidOperationException(
                    "Workflow routing failed after successful execution.",
                    ex);
            }
        }

        // Phase 3: Final outcome
        if (!success && coreException is not null)
            throw coreException;

        if (routingException is not null)
            throw routingException;
    }
}