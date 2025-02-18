using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Message;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Server.JsonProtocol;
using EmmyLua.LanguageServer.Framework.Server.RequestManager;
using EmmyLua.LanguageServer.Framework.Server.Scheduler;

namespace EmmyLua.LanguageServer.Framework.Server;

public class LanguageServer : LSPCommunicationBase
{
    enum RunningState
    {
        Running,
        Shutdown,
        Exit
    }

    private RunningState State { get; set; }

    public ClientProxy Client { get; }


    public LanguageServer(Stream input, Stream output) : base(input, output)
    {
        Client = new ClientProxy(this);
        AddHandler(new InitializeHandler(this));
    }

    public static LanguageServer From(Stream input, Stream output) => new(input, output);

    public delegate Task InitializeEvent(InitializeParams request, ServerInfo serverInfo);

    internal InitializeEvent? InitializeEventDelegate;

    public void OnInitialize(InitializeEvent handler)
    {
        InitializeEventDelegate += handler;
    }

    public delegate Task InitializedEvent(InitializedParams request);

    internal InitializedEvent? InitializedEventDelegate;

    public void OnInitialized(InitializedEvent handler)
    {
        InitializedEventDelegate += handler;
    }

    public delegate Task ShutdownEvent();

    internal ShutdownEvent? ShutdownEventDelegate;

    public void OnShutdown(ShutdownEvent handler)
    {
        ShutdownEventDelegate += handler;
    }

    // public delegate void StartEvent();
    //
    // internal StartEvent? StartEventDelegate;
    //
    // public void OnStart(StartEvent handler)
    // {
    //     StartEventDelegate += handler;
    // }

    protected override bool BaseHandle(Message message)
    {
        if (message is RequestMessage requestMessage)
        {
            if (requestMessage.Method == "shutdown")
            {
                State = RunningState.Shutdown;
                ShutdownEventDelegate?.Invoke();
                Writer.WriteResponse(requestMessage.Id, null);
                return true;
            }
        }
        else if (message is NotificationMessage notification)
        {
            if (notification.Method == "exit")
            {
                State = RunningState.Exit;
                // Environment.Exit(State == RunningState.Shutdown ? 0 : 1);
            }
            else if (notification.Method == "$/cancelRequest")
            {
                var cancelParams = notification.Params?.Deserialize<CancelParams>(JsonSerializerOptions);
                if (cancelParams != null)
                {
                    ClientRequestTokenManager.CancelToken(cancelParams.Id);
                }

                return true;
            }
        }

        if (State != RunningState.Running)
        {
            return true;
        }

        return false;
    }
}
