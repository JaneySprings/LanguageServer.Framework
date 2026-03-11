namespace EmmyLua.LanguageServer.Framework.Server;

/// <summary>
/// Language Server configuration options
/// </summary>
public class LanguageServerOptions
{
    /// <summary>
    /// Default request timeout (default: 30 seconds)
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// JSON protocol read buffer size (default: 1024 bytes)
    /// </summary>
    public int ReadBufferSize { get; set; } = 1024;

    /// <summary>
    /// Whether to output detailed stack traces on exceptions (default: true)
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = true;

    /// <summary>
    /// Timeout for waiting for tasks to complete when shutting down the scheduler (default: 5 seconds)
    /// </summary>
    public TimeSpan SchedulerShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Whether to enable performance tracing (default: false)
    /// When enabled, a DefaultPerformanceMetricsCollector will be automatically created to collect performance metrics.
    /// You can retrieve the current performance metrics via languageServer.GetMetrics().
    /// </summary>
    public bool EnablePerformanceTracing { get; set; } = false;

    /// <summary>
    /// Performance metrics print interval (null means no automatic printing, default: null)
    /// When set, performance metrics will be printed to stderr periodically
    /// For example: TimeSpan.FromMinutes(5) prints every 5 minutes
    /// </summary>
    public TimeSpan? PerformanceMetricsPrintInterval { get; set; } = null;

    /// <summary>
    /// Maximum number of concurrent requests (0 means unlimited, default: 0)
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 0;

    /// <summary>
    /// Create default configuration
    /// </summary>
    public static LanguageServerOptions Default => new();
}
