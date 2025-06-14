﻿using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class InlayHintHandlerBase : IJsonHandler
{
    protected abstract Task<InlayHintResponse?> Handle(InlayHintParams request, CancellationToken cancellationToken);

    protected abstract Task<InlayHint> Resolve(InlayHint request, CancellationToken cancellationToken);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/inlayHint", async (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InlayHintParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, cancelToken);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
        lspCommunication.AddRequestHandler("inlayHint/resolve", async (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InlayHint>(lspCommunication.JsonSerializerOptions)!;
            var r = await Resolve(request, cancelToken);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
