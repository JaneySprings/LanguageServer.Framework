namespace EmmyLua.LanguageServer.Framework.Server;

/// <summary>
/// Language Server 配置选项
/// </summary>
public class LanguageServerOptions
{
    /// <summary>
    /// 请求默认超时时间（默认：30秒）
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// JSON 协议读取缓冲区大小（默认：1024字节）
    /// </summary>
    public int ReadBufferSize { get; set; } = 1024;

    /// <summary>
    /// 是否在异常时输出详细堆栈跟踪（默认：true）
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = true;

    /// <summary>
    /// Scheduler 关闭时等待任务完成的超时时间（默认：5秒）
    /// </summary>
    public TimeSpan SchedulerShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 是否启用性能追踪（默认：false）
    /// </summary>
    public bool EnablePerformanceTracing { get; set; } = false;

    /// <summary>
    /// 最大并发请求数量（0表示无限制，默认：0）
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 0;

    /// <summary>
    /// 创建默认配置
    /// </summary>
    public static LanguageServerOptions Default => new();
}
