using System.Text.Json.Nodes;

namespace WJb.Actions;

// Invariant:
// - Actions control their own execution lifecycle.
// - Workflow routing is explicit and action-owned.
// - JobProcessor executes actions but never orchestrates them.

public abstract class WorkflowActionBase : IAction
{
    protected abstract Task ExecCoreAsync(JsonObject? jobMore, CancellationToken stoppingToken);

    protected abstract Task ExecNextAsync(bool success, JsonObject jobMore, CancellationToken stoppingToken);

    public async Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        // Design principle:
        // Simplicity → Explicitness → Extensibility
        //
        // Invariant:
        // - Core is executed exactly once
        // - Next is executed exactly once
        // - Execution result is never hidden

        var success = false; Exception? coreException = null;

        try
        {
            await ExecCoreAsync(jobMore, stoppingToken)
                .ConfigureAwait(false);

            success = true;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            coreException = null;
            success = false;
            throw;
        }
        catch (Exception ex)
        {
            coreException = ex;
            success = false;
        }
        finally
        {
            try
            {
                await ExecNextAsync(success, jobMore?.DeepClone() as JsonObject ?? [], stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // cancellation is propagated
                if (coreException is null)
                    throw;
            }
            catch (Exception nextEx)
            {
                // Core succeeded but Next failed → workflow error
                if (success)
                    throw new InvalidOperationException("Workflow routing failed after successful execution.", nextEx);
            }
        }

        if (!success && coreException is not null)
            throw coreException;
    }
}