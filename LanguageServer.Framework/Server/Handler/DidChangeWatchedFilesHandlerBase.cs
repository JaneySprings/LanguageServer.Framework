using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Registration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile.Watch;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DidChangeWatchedFilesHandlerBase : IJsonHandler
{
    protected abstract Task Handle(DidChangeWatchedFilesParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddNotificationHandler("workspace/didChangeWatchedFiles", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidChangeWatchedFilesParams>(lspCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
