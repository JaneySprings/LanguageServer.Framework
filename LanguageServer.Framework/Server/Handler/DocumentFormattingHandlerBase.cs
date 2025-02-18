using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentFormatting;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DocumentFormattingHandlerBase : IJsonHandler
{
    protected abstract Task<DocumentFormattingResponse?> Handle(DocumentFormattingParams request,
        CancellationToken token);

    protected abstract Task<DocumentFormattingResponse?> Handle(DocumentRangeFormattingParams request,
        CancellationToken token);

    protected abstract Task<DocumentFormattingResponse?> Handle(DocumentRangesFormattingParams request,
        CancellationToken token);

    protected abstract Task<DocumentFormattingResponse?> Handle(DocumentOnTypeFormattingParams request,
        CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/formatting", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentFormattingParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("textDocument/rangeFormatting", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentRangeFormattingParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("textDocument/rangesFormatting", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentRangesFormattingParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("textDocument/onTypeFormatting", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentOnTypeFormattingParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
    }


    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
