using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentColor;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DocumentColorHandlerBase : IJsonHandler
{
    protected abstract Task<DocumentColorResponse> Handle(DocumentColorParams request, CancellationToken token);

    protected abstract Task<ColorPresentationResponse> Resolve(ColorPresentationParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/documentColor", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentColorParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("textDocument/colorPresentation", async (message, token) =>
        {
            var request = message.Params!.Deserialize<ColorPresentationParams>(lSPCommunication.JsonSerializerOptions)!;
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
