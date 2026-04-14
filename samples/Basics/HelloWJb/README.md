# **1stWJb – Minimal Console App**

The simplest example showing how to enqueue and execute a job using **WJb**.

***

## How it works

### Register actions and job processor

```csharp
services.AddSingleton<PrintAction>();

services.AddSingleton<IActionFactory>(sp =>
    new ActionFactory(sp,
        new Dictionary<string, ActionItem>
        {
            ["print"] = new ActionItem(
                typeof(PrintAction).AssemblyQualifiedName!,
                null)
        }));

services.AddSingleton<JobProcessor>();
services.AddSingleton<IJobProcessor>(
    sp => sp.GetRequiredService<JobProcessor>());
services.AddHostedService(
    sp => sp.GetRequiredService<JobProcessor>());
```

*   Actions are identified by a string (`"print"`)
*   `JobProcessor` runs as a hosted service

***

### Enqueue a job at startup

```csharp
var jobs = host.Services.GetRequiredService<IJobProcessor>();

var job = await jobs.CompactAsync(
    "print",
    new { text = "Hello WJb!" });

await jobs.EnqueueJobAsync(job);
```

This creates and enqueues a job before the host starts running.

***

### Action implementation

```csharp
public sealed class PrintAction(ILogger<PrintAction> logger) : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken cancellationToken)
    {
        var text = jobMore?["text"]?.GetValue<string>() ?? "<empty>";
        logger.LogInformation(text);
        return Task.CompletedTask;
    }
}
```

*   Actions implement `IAction`
*   `jobMore` contains the job data
*   Logging uses `ILogger`

***

## 📌 **Output**

```text
info: WJb.JobProcessor[0] JobProcessor started
info: PrintAction[0] Hello WJb!
```

***
