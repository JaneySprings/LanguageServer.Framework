using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class SemanticTokensHandlerBase : IJsonHandler
{
    protected abstract Task<SemanticTokens?> Handle(SemanticTokensParams semanticTokensParams,
        CancellationToken cancellationToken);

    protected abstract Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams semanticTokensDeltaParams,
        CancellationToken cancellationToken);

    protected abstract Task<SemanticTokens?> Handle(SemanticTokensRangeParams semanticTokensRangeParams,
        CancellationToken cancellationToken);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/semanticTokens/full", async (message, token) =>
        {
            var request = message.Params!.Deserialize<SemanticTokensParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("textDocument/semanticTokens/full/delta", async (message, token) =>
        {
            var request = message.Params!.Deserialize<SemanticTokensDeltaParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("textDocument/semanticTokens/range", async (message, token) =>
        {
            var request = message.Params!.Deserialize<SemanticTokensRangeParams>(lSPCommunication.JsonSerializerOptions)!;
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
