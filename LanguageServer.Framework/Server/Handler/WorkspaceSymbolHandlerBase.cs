using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceSymbol;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class WorkspaceSymbolHandlerBase : IJsonHandler
{
    protected abstract Task<WorkspaceSymbolResponse> Handle(WorkspaceSymbolParams request, CancellationToken token);

    protected abstract Task<WorkspaceSymbol> Resolve(WorkspaceSymbol request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("workspace/symbol", async (message, token) =>
        {
            var request = message.Params!.Deserialize<WorkspaceSymbolParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("workspaceSymbol/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<WorkspaceSymbol>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Resolve(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
