using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DocumentSymbolHandlerBase : IJsonHandler
{
    protected abstract Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/documentSymbol", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentSymbolParams>(lSPCommunication.JsonSerializerOptions)!;
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
