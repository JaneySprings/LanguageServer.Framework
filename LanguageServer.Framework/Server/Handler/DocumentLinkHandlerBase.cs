﻿using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentLink;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DocumentLinkHandlerBase : IJsonHandler
{
    protected abstract Task<DocumentLinkResponse> Handle(DocumentLinkParams request, CancellationToken token);

    protected abstract Task<DocumentLink> Resolve(DocumentLink request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/documentLink", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentLinkParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("documentLink/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DocumentLink>(lspCommunication.JsonSerializerOptions)!;
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
