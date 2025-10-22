using System.Collections.Concurrent;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Server.RequestManager;

public class ServerRequestManager
{
    private ConcurrentDictionary<int, TaskCompletionSource<JsonDocument?>> _intRequestTokens = new();

    private int _idCount = 0;

    /// <summary>
    /// 默认请求超时时间（30秒）
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public RequestMessage MakeRequest(string method, JsonDocument? @params)
    {
        var id = Interlocked.Increment(ref _idCount);
        _intRequestTokens[id] = new TaskCompletionSource<JsonDocument?>();
        return new RequestMessage(id, method, @params);
    }

    public void OnResponse(StringOrInt id, JsonDocument? response)
    {
        if (id.StringValue is null)
        {
            var intId = id.IntValue;
            if (_intRequestTokens.TryRemove(intId, out var tcs))
            {
                tcs.TrySetResult(response);
            }
        }
    }

    /// <summary>
    /// 等待响应，使用默认超时时间
    /// </summary>
    public Task<JsonDocument?> WaitResponse(StringOrInt id, CancellationToken token)
    {
        return WaitResponse(id, DefaultTimeout, token);
    }

    /// <summary>
    /// 等待响应，指定超时时间
    /// </summary>
    public async Task<JsonDocument?> WaitResponse(StringOrInt id, TimeSpan timeout, CancellationToken token)
    {
        if (id.StringValue is null)
        {
            var intId = id.IntValue;
            if (_intRequestTokens.TryGetValue(intId, out var tcs))
            {
                // 创建组合超时令牌
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeoutCts.CancelAfter(timeout);

                await using (timeoutCts.Token.Register(() =>
                             {
                                 if (tcs.TrySetCanceled())
                                 {
                                     _intRequestTokens.TryRemove(intId, out _);
                                 }
                             }).ConfigureAwait(false))
                {
                    try
                    {
                        var result = await tcs.Task.ConfigureAwait(false);
                        _intRequestTokens.TryRemove(intId, out _);
                        return result;
                    }
                    catch (OperationCanceledException)
                    {
                        _intRequestTokens.TryRemove(intId, out _);

                        // 区分是超时还是主动取消
                        if (token.IsCancellationRequested)
                        {
                            throw;
                        }
                        else
                        {
                            throw new TimeoutException($"Request {intId} timed out after {timeout.TotalSeconds} seconds");
                        }
                    }
                }
            }
        }

        throw new InvalidOperationException($"Invalid response id: {id}");
    }

    /// <summary>
    /// 清理挂起的请求
    /// </summary>
    public void CancelPendingRequests()
    {
        foreach (var kvp in _intRequestTokens)
        {
            if (_intRequestTokens.TryRemove(kvp.Key, out var tcs))
            {
                tcs.TrySetCanceled();
            }
        }
    }
}
