using System.Buffers;
using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolReader(Stream inputStream, JsonSerializerOptions jsonSerializerOptions)
{
    private int _currentValidLength = 0;

    private byte[] SmallBuffer { get; } = new byte[1024];

    public async Task<Message> ReadAsync(CancellationToken token = default)
    {
        // Read the header part
        var (totalLength, contentStart) = await ReadOneHeaderAsync(token);
        var readContentLength = _currentValidLength - contentStart;
        if (readContentLength > totalLength)
        {
            readContentLength = totalLength;
        }

        try
        {
            // 对于小消息使用 SmallBuffer 避免 ArrayPool 的开销
            if (totalLength + contentStart <= SmallBuffer.Length)
            {
                return await ReadSmallJsonRpcMessageAsync(totalLength, contentStart, readContentLength, token);
            }
            else
            {
                return await ReadLargeJsonRpcMessageAsync(totalLength, contentStart, readContentLength, token);
            }
        }
        finally
        {
            // 使用 totalLength 而不是 readContentLength，因为消息可能已经完全读取
            if (contentStart + totalLength < _currentValidLength)
            {
                var remaining = _currentValidLength - (contentStart + totalLength);
                Array.Copy(SmallBuffer, contentStart + totalLength, SmallBuffer, 0, remaining);
                _currentValidLength = remaining;
            }
            else
            {
                _currentValidLength = 0;
            }
        }
    }

    private async Task ReadHeaderToBufferAsync(CancellationToken token)
    {
        var read = await inputStream.ReadAsync(SmallBuffer.AsMemory(_currentValidLength), token);
        if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
        _currentValidLength += read;
    }

    private bool TryGetContentLength(int startIndex, out int contentLength, out int contentStart)
    {
        contentLength = 0;
        contentStart = 0;

        var buffer = SmallBuffer.AsSpan(startIndex, _currentValidLength - startIndex);

        for (var i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == '\r' && i + 1 < buffer.Length && buffer[i + 1] == '\n')
            {
                var headerEnd = i;
                if (headerEnd > 0)
                {
                    // 使用 Span 避免字符串分配
                    var headerSpan = buffer[..headerEnd];

                    // 直接在 byte span 上查找 "Content-Length:"
                    ReadOnlySpan<byte> contentLengthPrefix = "Content-Length:"u8;

                    if (headerSpan.Length >= contentLengthPrefix.Length &&
                        headerSpan[..contentLengthPrefix.Length].SequenceEqual(contentLengthPrefix))
                    {
                        // 解析数字部分
                        var numberSpan = headerSpan[contentLengthPrefix.Length..];

                        // 跳过空格
                        int numberStart = 0;
                        while (numberStart < numberSpan.Length && numberSpan[numberStart] == ' ')
                        {
                            numberStart++;
                        }

                        if (numberStart < numberSpan.Length)
                        {
                            // 使用 Utf8Parser 高效解析数字
                            if (TryParseInt32(numberSpan[numberStart..], out contentLength))
                            {
                                // 找到了 Content-Length
                            }
                            else
                            {
                                throw new InvalidOperationException("Invalid Content-Length header.");
                            }
                        }
                    }
                }

                var nextLineStart = startIndex + i + 2;
                if (nextLineStart + 1 < _currentValidLength &&
                    SmallBuffer[nextLineStart] == '\r' &&
                    SmallBuffer[nextLineStart + 1] == '\n')
                {
                    contentStart = nextLineStart + 2;
                    return true;
                }
            }
        }

        return false;
    }

    // 快速 UTF8 整数解析
    private static bool TryParseInt32(ReadOnlySpan<byte> utf8Bytes, out int value)
    {
        value = 0;
        if (utf8Bytes.Length == 0) return false;

        foreach (var b in utf8Bytes)
        {
            if (b < '0' || b > '9')
            {
                return value > 0; // 如果已经解析了一些数字，返回 true
            }
            value = value * 10 + (b - '0');
        }
        return true;
    }

    private async Task<(int, int)> ReadOneHeaderAsync(CancellationToken token)
    {
        var totalLength = 0;
        var contentStart = 0;

        while (true)
        {
            if (TryGetContentLength(0, out totalLength, out contentStart))
            {
                break;
            }

            await ReadHeaderToBufferAsync(token);
        }

        return (totalLength, contentStart);
    }

    private async Task<Message> ReadSmallJsonRpcMessageAsync(int totalContentLength, int contentStart,
        int readContentLength, CancellationToken token = default)
    {
        try
        {
            // 继续读取剩余的消息内容
            while (readContentLength < totalContentLength)
            {
                var bytesToRead = totalContentLength - readContentLength;
                var availableSpace = SmallBuffer.Length - (contentStart + readContentLength);
                if (bytesToRead > availableSpace)
                {
                    throw new InvalidOperationException("Buffer overflow: message too large for small buffer.");
                }

                var read = await inputStream.ReadAsync(
                    SmallBuffer.AsMemory(contentStart + readContentLength, bytesToRead), token);
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                readContentLength += read;

                // 更新 _currentValidLength 以反映实际读取的数据
                var newValidLength = contentStart + readContentLength;
                if (newValidLength > _currentValidLength)
                {
                    _currentValidLength = newValidLength;
                }
            }

            return JsonSerializer.Deserialize<Message>(SmallBuffer.AsSpan(contentStart, totalContentLength),
                jsonSerializerOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON-RPC message.", ex);
        }
    }

    private async Task<Message> ReadLargeJsonRpcMessageAsync(int totalContentLength, int contentStart,
        int readContentLength, CancellationToken token = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(totalContentLength);
        // 将smallbuffer中的数据拷贝到buffer中
        SmallBuffer.AsSpan(contentStart, readContentLength).CopyTo(buffer);
        try
        {
            var bytesRead = readContentLength;
            while (bytesRead < totalContentLength)
            {
                var bufferSpan = buffer.AsMemory(bytesRead, totalContentLength - bytesRead);
                var read = await inputStream.ReadAsync(bufferSpan, token);
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                bytesRead += read;
            }

            return JsonSerializer.Deserialize<Message>(buffer.AsSpan(0, totalContentLength),
                jsonSerializerOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON-RPC message.", ex);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
