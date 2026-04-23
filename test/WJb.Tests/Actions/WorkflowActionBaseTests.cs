using System.Text.Json.Nodes;

namespace WJb.Actions.Tests;

public sealed class WorkflowActionBaseTests
{
    [Fact]
    public async Task ExecAsync_Calls_Core_And_Next_Once_On_Success()
    {
        var action = new TestWorkflowAction();

        await action.ExecAsync(new JsonObject(), CancellationToken.None);

        Assert.Equal(1, action.CoreCalls);
        Assert.Equal(1, action.NextCalls);
        Assert.True(action.LastSuccess);
    }

    [Fact]
    public async Task ExecAsync_Calls_Next_When_Core_Throws()
    {
        var action = new TestWorkflowAction
        {
            CoreException = new InvalidOperationException("boom")
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            action.ExecAsync(new JsonObject(), CancellationToken.None));

        Assert.Equal("boom", ex.Message);
        Assert.Equal(1, action.CoreCalls);
        Assert.Equal(1, action.NextCalls);
        Assert.False(action.LastSuccess);
    }

    [Fact]
    public async Task ExecAsync_Swallows_Next_Exception_When_Core_Fails()
    {
        var action = new TestWorkflowAction
        {
            CoreException = new InvalidOperationException("boom"),
            NextException = new InvalidOperationException("next-fail")
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            action.ExecAsync(new JsonObject(), CancellationToken.None));

        Assert.Equal("boom", ex.Message);
        Assert.Equal(1, action.CoreCalls);
        Assert.Equal(1, action.NextCalls);
    }

    [Fact]
    public async Task ExecAsync_Throws_When_Next_Fails_After_Success()
    {
        var action = new TestWorkflowAction
        {
            NextException = new InvalidOperationException("next-fail")
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            action.ExecAsync(new JsonObject(), CancellationToken.None));

        Assert.Contains("Workflow routing failed", ex.Message);
        Assert.Equal(1, action.CoreCalls);
        Assert.Equal(1, action.NextCalls);
    }

    [Fact]
    public async Task ExecAsync_Propagates_Cancellation_From_Core()
    {
        var action = new TestWorkflowAction
        {
            CancelInCore = true
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            action.ExecAsync(new JsonObject(), cts.Token));

        Assert.Equal(1, action.CoreCalls);
        Assert.Equal(1, action.NextCalls);
    }

    [Fact]
    public async Task ExecAsync_Propagates_Cancellation_From_Next_When_Core_Succeeded()
    {
        var action = new TestWorkflowAction
        {
            CancelInNext = true
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            action.ExecAsync(new JsonObject(), cts.Token));

        Assert.Equal(1, action.CoreCalls);
        Assert.Equal(1, action.NextCalls);
    }
}

/* =======================
   Test helper action
   ======================= */

internal sealed class TestWorkflowAction : WorkflowActionBase
{
    public int CoreCalls;
    public int NextCalls;

    public bool LastSuccess;

    public Exception? CoreException;
    public Exception? NextException;

    public bool CancelInCore;
    public bool CancelInNext;

    protected override Task ExecCoreAsync(
        JsonObject? jobMore,
        CancellationToken stoppingToken)
    {
        CoreCalls++;

        if (CancelInCore)
            stoppingToken.ThrowIfCancellationRequested();

        if (CoreException != null)
            throw CoreException;

        return Task.CompletedTask;
    }

    protected override Task ExecNextAsync(
        bool success,
        JsonObject jobMore,
        CancellationToken stoppingToken)
    {
        NextCalls++;
        LastSuccess = success;

        if (CancelInNext)
            stoppingToken.ThrowIfCancellationRequested();

        if (NextException != null)
            throw NextException;

        return Task.CompletedTask;
    }
}
