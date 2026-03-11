using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceSymbol;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class WorkspaceSymbolHandlerBase : IJsonHandler
{
    protected abstract Task<WorkspaceSymbolResponse> Handle(WorkspaceSymbolParams request, CancellationToken token);

    protected abstract Task<WorkspaceSymbol> Resolve(WorkspaceSymbol request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("workspace/symbol", async (message, token) =>
        {
            var request = message.Params!.Deserialize<WorkspaceSymbolParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("workspaceSymbol/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<WorkspaceSymbol>(lspCommunication.JsonSerializerOptions)!;
            var r = await Resolve(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
