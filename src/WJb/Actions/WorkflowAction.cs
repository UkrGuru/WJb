using System.Text.Json.Nodes;
using WJb.Extensions;

namespace WJb.Actions;

/// <summary>
/// Base class for workflow-style actions.
/// 
/// Invariants:
/// - Actions control their own execution lifecycle.
/// - Workflow routing is explicit and action-owned.
/// - JobProcessor executes actions but never orchestrates them.
///
/// Execution model:
/// 1. Core execution (ExecCoreAsync) is executed exactly once.
/// 2. Routing execution (ExecNextAsync) is executed exactly once.
/// 3. Routing always receives a deep-cloned payload.
/// 4. Core and routing failures are isolated and propagated deterministically.
/// </summary>
public abstract class WorkflowActionBase : IAction
{
    /// <summary>
    /// Core business logic of the action.
    /// Executed exactly once.
    /// </summary>
    protected abstract Task ExecCoreAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken);

    /// <summary>
    /// Workflow routing logic executed after core execution.
    /// 
    /// jobMore is a deep clone of the original payload and
    /// is always non-null and safe to mutate.
    /// </summary>
    protected abstract Task ExecNextAsync(
        bool success,
        JsonObject jobMore,
        CancellationToken stoppingToken);

    /// <summary>
    /// Unified execution entry point.
    /// Controls the full workflow lifecycle.
    /// </summary>
    public async Task ExecAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken)
    {
        bool success = false;

        Exception? coreException = null;
        Exception? routingException = null;

        // -------------------------------------------------
        // Phase 1: Core execution
        // -------------------------------------------------

        try
        {
            await ExecCoreAsync(jobMore, stoppingToken)
                .ConfigureAwait(false);

            success = true;
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Cancellation is always authoritative
            throw;
        }
        catch (Exception ex)
        {
            coreException = ex;
            success = false;
        }

        // -------------------------------------------------
        // Phase 2: Routing execution (always executed)
        // -------------------------------------------------

        try
        {
            var nextMore = jobMore?.DeepClone() as JsonObject ?? [];

            await ExecNextAsync(
                success,
                nextMore,
                stoppingToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Routing cancellation only matters
            // if core did not already fail
            if (coreException is null)
                routingException =
                    new OperationCanceledException(stoppingToken);
        }
        catch (Exception ex)
        {
            // Routing exceptions only propagate
            // if core execution succeeded
            if (success)
            {
                routingException =
                    new InvalidOperationException(
                        "Workflow routing failed after successful execution.",
                        ex);
            }
        }

        // -------------------------------------------------
        // Phase 3: Final outcome
        // -------------------------------------------------

        if (!success && coreException is not null)
            throw coreException;

        if (routingException is not null)
            throw routingException;
    }
}