using System.Text.Json.Nodes;

namespace WJb;

/// <summary>
/// Describes an action definition and its default metadata.
/// </summary>
public class ActionItem
{
    /// <summary>
    /// Fully qualified CLR type name of the action.
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Default metadata applied to the action.
    /// </summary>
    public JsonObject? More { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionItem"/> class.
    /// </summary>
    public ActionItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionItem"/> class with the specified type and metadata.
    /// </summary>

    public ActionItem(string type, JsonObject? more)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Action type must be specified.", nameof(type));

        // Type name is mandatory for action resolution
        Type = type;

        // Metadata may be null and merged later at runtime
        More = more;
    }
}

/// <summary>
/// Represents an executable action.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Executes the action with provided metadata.
    /// </summary>
    Task ExecAsync(JsonObject? payload, CancellationToken cancellationToken);
}

/// <summary>
/// Factory for creating actions and accessing action metadata.
/// </summary>
public interface IActionFactory : IActionRegistry
{
    /// <summary>
    /// Expands a job payload into an action code and metadata.
    /// </summary>
    IAction Create(string actionCode);

    /// <summary>
    /// Returns action metadata by logical action code.
    /// </summary>
    ActionItem GetActionItem(string actionCode);
}

/// <summary>
/// Registry interface for runtime-manageable action configurations.
/// </summary>
public interface IActionRegistry
{
    /// <summary>
    /// Returns a snapshot of the current action configuration.
    /// </summary>
    IReadOnlyDictionary<string, ActionItem> Snapshot();
}

/// <summary>
/// Processes jobs from a queue and executes actions.
/// </summary>
public interface IJobProcessor
{
    /// <summary>
    /// Compacts action code and metadata into a job payload.
    /// </summary>
    Task<string> CompactAsync(string actionCode, object? jobMore = null, CancellationToken stoppingToken = default);

    /// <summary>
    /// Enqueues a job for processing.
    /// </summary>
    Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default);

    /// <summary>
    /// Expands a job payload into action code and metadata.
    /// </summary>
    Task<(string Code, JsonObject More)> ExpandAsync(string job, CancellationToken stoppingToken = default);

    /// <summary>
    /// Processes a single job payload.
    /// </summary>
    Task ProcessJobAsync(string job, CancellationToken stoppingToken = default);
}

/// <summary>
/// Priority-aware job queue abstraction.
/// </summary>
public interface IJobQueue
{
    /// <summary>
    /// Enqueues a job with the specified priority.
    /// </summary>
    Task EnqueueAsync(string job, Priority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the next available job respecting priority order.
    /// </summary>
    Task<string> DequeueNextAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for cron-based job scheduling.
/// Represents a single scheduling decision step, independent of hosting lifecycle.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Evaluates configured cron expressions at the specified point in time
    /// and enqueues all due jobs exactly once.
    /// </summary>
    /// <remarks>
    /// This method represents the core scheduling contract.
    /// Hosting lifetime, delays, reloads, and threading are intentionally excluded.
    /// </remarks>
    Task ProcessDueCronActionsAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a cron expression is due at the specified time.
    /// </summary>
    /// <remarks>
    /// Cron semantics are delegated to <see cref="CronHelper"/>.
    /// Implementations must not interpret cron expressions themselves.
    /// </remarks>
    bool IsDue(string? cron, DateTime now);
}

/// <summary>
/// Provides access to application settings by key.
/// </summary>
public interface ISettingsRegistry
{
    /// <summary>
    /// Retrieves a setting value by key.
    /// </summary>
    T Get<T>(string key, T defaultValue = default!);
}

/// <summary>
/// Represents an action capable of explicitly routing workflow execution
/// by enqueuing the next job.
/// </summary>
public interface IWorkflowAction
{
    /// <summary>
    /// Routes workflow execution to the next action using provided metadata.
    /// </summary>
    Task NextAsync(JsonObject? nextMore, CancellationToken stoppingToken = default);

    // Workflow routing is explicit and opt-in.
    // Only actions that implement IWorkflowAction
    // are allowed to enqueue follow-up jobs.
}

/// <summary>
/// Priority levels for job execution.
/// </summary>
public enum Priority
{
    /// <summary>
    /// Execute as soon as possible (highest priority).
    /// </summary>
    ASAP,

    /// <summary>
    /// High priority.
    /// </summary>
    High,

    /// <summary>
    /// Normal/default priority.
    /// </summary>
    Normal,

    /// <summary>
    /// Low priority (lowest).
    /// </summary>
    Low
}

