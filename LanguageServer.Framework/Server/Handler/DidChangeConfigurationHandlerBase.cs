using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Configuration;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DidChangeConfigurationHandlerBase : IJsonHandler
{
    protected abstract Task Handle(DidChangeConfigurationParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddNotificationHandler("workspace/didChangeConfiguration", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidChangeConfigurationParams>(lspCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
