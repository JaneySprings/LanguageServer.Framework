using System.Collections.Concurrent;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.Scheduler;

public class SingleThreadScheduler : IScheduler, IDisposable
{
    private readonly Thread _workerThread;
    private readonly BlockingCollection<Action> _tasks = new();
    private readonly TimeSpan _shutdownTimeout;
    private bool _disposed;

    public SingleThreadScheduler()
        : this(LanguageServerOptions.Default.SchedulerShutdownTimeout)
    {
    }

    public SingleThreadScheduler(TimeSpan shutdownTimeout)
    {
        _shutdownTimeout = shutdownTimeout;
        _workerThread = new Thread(() =>
        {
            foreach (var task in _tasks.GetConsumingEnumerable())
            {
                try
                {
                    task();
                }
                catch (Exception ex)
                {
                    // Log the exception instead of silently swallowing it
                    Console.Error.WriteLine($"[SingleThreadScheduler] Error executing task: {ex}");
                }
            }
        })
        {
            IsBackground = true,
            Name = "LSP-Scheduler-Thread"
        };
        _workerThread.Start();
    }

    public void Schedule(Func<Message, Task> action, Message message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _tasks.Add(() =>
        {
            try
            {
                action(message).Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SingleThreadScheduler] Error in async task: {ex}");
            }
        });
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _tasks.CompleteAdding();

        // Wait for the worker thread to finish processing remaining tasks
        if (_workerThread.IsAlive)
        {
            _workerThread.Join(_shutdownTimeout);
        }

        _tasks.Dispose();
    }
}
