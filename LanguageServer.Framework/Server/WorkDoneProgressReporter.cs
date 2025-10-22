using EmmyLua.LanguageServer.Framework.Protocol.Message.Progress;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;

namespace EmmyLua.LanguageServer.Framework.Server;

/// <summary>
/// Helper class for managing work done progress reporting.
/// </summary>
public class WorkDoneProgressReporter : IDisposable
{
    private readonly ClientProxy _client;
    private readonly StringOrInt _token;
    private bool _disposed;
    private bool _started;

    public WorkDoneProgressReporter(ClientProxy client, StringOrInt token)
    {
        _client = client;
        _token = token;
    }

    /// <summary>
    /// Begin the progress operation.
    /// </summary>
    public async Task Begin(string title, bool cancellable = false, string? message = null, uint? percentage = null)
    {
        if (_started)
            throw new InvalidOperationException("Progress already started");

        _started = true;
        await _client.ReportProgress(_token, new WorkDoneProgressBegin
        {
            Title = title,
            Cancellable = cancellable,
            Message = message,
            Percentage = percentage
        });
    }

    /// <summary>
    /// Report progress update.
    /// </summary>
    public async Task Report(string? message = null, uint? percentage = null, bool? cancellable = null)
    {
        if (!_started)
            throw new InvalidOperationException("Progress not started. Call Begin() first.");

        await _client.ReportProgress(_token, new WorkDoneProgressReport
        {
            Message = message,
            Percentage = percentage,
            Cancellable = cancellable
        });
    }

    /// <summary>
    /// End the progress operation.
    /// </summary>
    public async Task End(string? message = null)
    {
        if (!_started)
            return;

        if (_disposed)
            return;

        await _client.ReportProgress(_token, new WorkDoneProgressEnd
        {
            Message = message
        });

        _disposed = true;
    }

    public void Dispose()
    {
        if (!_disposed && _started)
            // Fire and forget
            _ = End();
    }

    /// <summary>
    /// Create a new work done progress reporter.
    /// </summary>
    public static async Task<WorkDoneProgressReporter> Create(
        ClientProxy client,
        string title,
        bool cancellable = false,
        string? message = null,
        uint? percentage = null,
        CancellationToken cancellationToken = default)
    {
        var token = new StringOrInt(Guid.NewGuid().ToString());
        await client.CreateWorkDoneProgress(token, cancellationToken);

        var reporter = new WorkDoneProgressReporter(client, token);
        await reporter.Begin(title, cancellable, message, percentage);
        return reporter;
    }
}
