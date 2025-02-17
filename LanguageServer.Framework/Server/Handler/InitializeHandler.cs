using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

internal class InitializeHandler(LanguageServer server) : IJsonHandler
{
    private Task<InitializeResult> Handle(InitializeParams request, CancellationToken cancellationToken)
    {
        var serverInfo = new ServerInfo();
        var capabilities = new ServerCapabilities();
        server.ClientCapabilities = request.Capabilities;
        foreach (var handler in server.Handlers)
        {
            handler.RegisterCapability(capabilities, request.Capabilities);
        }

        server.InitializeEventDelegate?.Invoke(request, serverInfo);
        var result = new InitializeResult
        {
            ServerInfo = serverInfo,
            Capabilities = capabilities
        };
        return Task.FromResult(result);
    }

    private Task Handle(InitializedParams request, CancellationToken cancellationToken)
    {
        server.InitializedEventDelegate?.Invoke(request);

        foreach (var handler in server.Handlers)
        {
            handler.RegisterDynamicCapability(server, server.ClientCapabilities);
        }
        return Task.CompletedTask;
    }

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("initialize", async (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InitializeParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, cancelToken);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
        lSPCommunication.AddNotificationHandler("initialized", (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InitializedParams>(lSPCommunication.JsonSerializerOptions)!;
            return Handle(request, cancelToken);
        });
    }

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
    }

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
