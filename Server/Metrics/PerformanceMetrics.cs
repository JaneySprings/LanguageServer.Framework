namespace EmmyLua.LanguageServer.Framework.Server.Metrics;

/// <summary>
/// Language Server performance metrics
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Total number of requests handled
    /// </summary>
    public long TotalRequestsHandled { get; set; }

    /// <summary>
    /// Total number of notifications handled
    /// </summary>
    public long TotalNotificationsHandled { get; set; }

    /// <summary>
    /// Total number of failed requests
    /// </summary>
    public long TotalRequestsFailed { get; set; }

    /// <summary>
    /// Average request processing time (in milliseconds)
    /// </summary>
    public double AverageRequestDurationMs { get; set; }

    /// <summary>
    /// Maximum request processing time (in milliseconds)
    /// </summary>
    public double MaxRequestDurationMs { get; set; }

    /// <summary>
    /// Current number of pending requests
    /// </summary>
    public int PendingRequestsCount { get; set; }

    /// <summary>
    /// Total number of messages sent
    /// </summary>
    public long TotalMessagesSent { get; set; }

    /// <summary>
    /// Total number of messages received
    /// </summary>
    public long TotalMessagesReceived { get; set; }

    /// <summary>
    /// Server start time
    /// </summary>
    public DateTime ServerStartTime { get; set; }

    /// <summary>
    /// Server uptime
    /// </summary>
    public TimeSpan Uptime => DateTime.UtcNow - ServerStartTime;

    /// <summary>
    /// Reset all metrics
    /// </summary>
    public void Reset()
    {
        TotalRequestsHandled = 0;
        TotalNotificationsHandled = 0;
        TotalRequestsFailed = 0;
        AverageRequestDurationMs = 0;
        MaxRequestDurationMs = 0;
        PendingRequestsCount = 0;
        TotalMessagesSent = 0;
        TotalMessagesReceived = 0;
        ServerStartTime = DateTime.UtcNow;
    }
}

/// <summary>
/// Performance metrics collector interface
/// </summary>
public interface IPerformanceMetricsCollector
{
    /// <summary>
    /// Get current performance metrics
    /// </summary>
    PerformanceMetrics GetMetrics();

    /// <summary>
    /// Record request start
    /// </summary>
    void RecordRequestStart(string method);

    /// <summary>
    /// Record request completion
    /// </summary>
    void RecordRequestComplete(string method, TimeSpan duration, bool success);

    /// <summary>
    /// Record notification handling
    /// </summary>
    void RecordNotification(string method);

    /// <summary>
    /// Record message sent
    /// </summary>
    void RecordMessageSent();

    /// <summary>
    /// Record message received
    /// </summary>
    void RecordMessageReceived();
}

/// <summary>
/// Default performance metrics collector implementation
/// </summary>
public class DefaultPerformanceMetricsCollector : IPerformanceMetricsCollector
{
    private readonly DateTime _serverStartTime = DateTime.UtcNow;
    private readonly object _lock = new();

    // Use separate fields to support Interlocked operations
    private long _totalRequestsHandled = 0;
    private long _totalNotificationsHandled = 0;
    private long _totalRequestsFailed = 0;
    private long _pendingRequestsCount = 0;
    private long _totalMessagesSent = 0;
    private long _totalMessagesReceived = 0;
    private long _totalDurationMs = 0;
    private double _maxRequestDurationMs = 0;

    public PerformanceMetrics GetMetrics()
    {
        lock (_lock)
        {
            var totalRequests = Interlocked.Read(ref _totalRequestsHandled);
            var totalDuration = Interlocked.Read(ref _totalDurationMs);

            return new PerformanceMetrics
            {
                TotalRequestsHandled = totalRequests,
                TotalNotificationsHandled = Interlocked.Read(ref _totalNotificationsHandled),
                TotalRequestsFailed = Interlocked.Read(ref _totalRequestsFailed),
                AverageRequestDurationMs = totalRequests > 0 ? totalDuration / (double)totalRequests : 0,
                MaxRequestDurationMs = _maxRequestDurationMs,
                PendingRequestsCount = (int)Interlocked.Read(ref _pendingRequestsCount),
                TotalMessagesSent = Interlocked.Read(ref _totalMessagesSent),
                TotalMessagesReceived = Interlocked.Read(ref _totalMessagesReceived),
                ServerStartTime = _serverStartTime
            };
        }
    }

    public void RecordRequestStart(string method)
    {
        Interlocked.Increment(ref _pendingRequestsCount);
    }

    public void RecordRequestComplete(string method, TimeSpan duration, bool success)
    {
        Interlocked.Decrement(ref _pendingRequestsCount);

        if (success)
        {
            Interlocked.Increment(ref _totalRequestsHandled);

            var durationMs = (long)duration.TotalMilliseconds;
            Interlocked.Add(ref _totalDurationMs, durationMs);

            // MaxRequestDurationMs needs lock protection
            lock (_lock)
            {
                if (duration.TotalMilliseconds > _maxRequestDurationMs)
                {
                    _maxRequestDurationMs = duration.TotalMilliseconds;
                }
            }
        }
        else
        {
            Interlocked.Increment(ref _totalRequestsFailed);
        }
    }

    public void RecordNotification(string method)
    {
        Interlocked.Increment(ref _totalNotificationsHandled);
    }

    public void RecordMessageSent()
    {
        Interlocked.Increment(ref _totalMessagesSent);
    }

    public void RecordMessageReceived()
    {
        Interlocked.Increment(ref _totalMessagesReceived);
    }
}
