using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class RenameHandlerBase : IJsonHandler
{
    protected abstract Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken token);

    protected abstract Task<PrepareRenameResponse> Handle(PrepareRenameParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/rename", async (message, token) =>
        {
            var request = message.Params!.Deserialize<RenameParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("textDocument/prepareRename", async (message, token) =>
        {
            var request = message.Params!.Deserialize<PrepareRenameParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
