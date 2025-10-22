using System.Buffers;
using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Framework.Server.Metrics;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolWriter : IDisposable
{
    private readonly Stream _output;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// 性能指标收集器
    /// </summary>
    public IPerformanceMetricsCollector? MetricsCollector { get; set; }

    public JsonProtocolWriter(Stream output, JsonSerializerOptions jsonSerializerOptions)
    {
        _output = output;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public void WriteResponse(StringOrInt id, JsonDocument? document, ResponseError? error = null)
    {
        var response = new ResponseMessage(id, document, error);
        WriteMessage(response);
    }

    public void WriteNotification(NotificationMessage message)
    {
        WriteMessage(message);
    }

    public void WriteRequest(RequestMessage message)
    {
        WriteMessage(message);
    }

    private void WriteMessage<T>(T message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // 使用线程安全的写入
        _writeLock.Wait();
        try
        {
            // 序列化到内存
            var json = JsonSerializer.Serialize(message, _jsonSerializerOptions);
            var contentLength = Encoding.UTF8.GetByteCount(json);

            // 计算header大小
            var headerText = $"Content-Length: {contentLength}\r\n\r\n";
            var headerBytes = Encoding.UTF8.GetByteCount(headerText);

            // 使用ArrayPool减少分配
            var totalLength = headerBytes + contentLength;
            var buffer = ArrayPool<byte>.Shared.Rent(totalLength);

            try
            {
                // 写入header
                var written = Encoding.UTF8.GetBytes(headerText, buffer);

                // 写入content
                written += Encoding.UTF8.GetBytes(json, buffer.AsSpan(written));

                // 一次性写入到流
                _output.Write(buffer, 0, written);
                _output.Flush();

                // 记录消息发送
                MetricsCollector?.RecordMessageSent();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _writeLock.Dispose();
    }
}
