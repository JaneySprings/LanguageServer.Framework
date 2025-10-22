namespace EmmyLua.LanguageServer.Framework.Server.Metrics;

/// <summary>
/// Language Server 性能指标
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// 已处理的请求总数
    /// </summary>
    public long TotalRequestsHandled { get; set; }

    /// <summary>
    /// 已处理的通知总数
    /// </summary>
    public long TotalNotificationsHandled { get; set; }

    /// <summary>
    /// 请求处理失败总数
    /// </summary>
    public long TotalRequestsFailed { get; set; }

    /// <summary>
    /// 平均请求处理时间（毫秒）
    /// </summary>
    public double AverageRequestDurationMs { get; set; }

    /// <summary>
    /// 最大请求处理时间（毫秒）
    /// </summary>
    public double MaxRequestDurationMs { get; set; }

    /// <summary>
    /// 当前待处理的请求数
    /// </summary>
    public int PendingRequestsCount { get; set; }

    /// <summary>
    /// 已发送的消息总数
    /// </summary>
    public long TotalMessagesSent { get; set; }

    /// <summary>
    /// 已接收的消息总数
    /// </summary>
    public long TotalMessagesReceived { get; set; }

    /// <summary>
    /// 服务器启动时间
    /// </summary>
    public DateTime ServerStartTime { get; set; }

    /// <summary>
    /// 服务器运行时间
    /// </summary>
    public TimeSpan Uptime => DateTime.UtcNow - ServerStartTime;

    /// <summary>
    /// 重置所有指标
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
/// 性能指标收集器接口
/// </summary>
public interface IPerformanceMetricsCollector
{
    /// <summary>
    /// 获取当前性能指标
    /// </summary>
    PerformanceMetrics GetMetrics();

    /// <summary>
    /// 记录请求开始
    /// </summary>
    void RecordRequestStart(string method);

    /// <summary>
    /// 记录请求完成
    /// </summary>
    void RecordRequestComplete(string method, TimeSpan duration, bool success);

    /// <summary>
    /// 记录通知处理
    /// </summary>
    void RecordNotification(string method);

    /// <summary>
    /// 记录消息发送
    /// </summary>
    void RecordMessageSent();

    /// <summary>
    /// 记录消息接收
    /// </summary>
    void RecordMessageReceived();
}

/// <summary>
/// 默认性能指标收集器实现
/// </summary>
public class DefaultPerformanceMetricsCollector : IPerformanceMetricsCollector
{
    private readonly DateTime _serverStartTime = DateTime.UtcNow;
    private readonly object _lock = new();

    // 使用独立字段以支持Interlocked操作
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

            // MaxRequestDurationMs 需要锁保护
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
