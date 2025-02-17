using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceFolders;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class WorkspaceFolderHandlerBase : IJsonHandler
{
    protected abstract Task Handle(DidChangeWorkspaceFoldersParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddNotificationHandler("workspace/didChangeWorkspaceFolders", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidChangeWorkspaceFoldersParams>(lSPCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
