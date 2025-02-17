using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentLink;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DocumentLinkHandlerBase : IJsonHandler
{
    protected abstract Task<DocumentLinkResponse> Handle(DocumentLinkParams request, CancellationToken token);

    protected abstract Task<DocumentLink> Resolve(DocumentLink request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/documentLink", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentLinkParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("documentLink/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentLink>(lSPCommunication.JsonSerializerOptions)!;
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
