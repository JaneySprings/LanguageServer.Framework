using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.InlineCompletion;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class InlineCompletionHandlerBase : IJsonHandler
{
    protected abstract Task<InlineCompletionResponse> Handle(InlineCompletionParams request,
        CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
         lSPCommunication.AddRequestHandler("textDocument/inlineCompletion", async (message, token) =>
         {
             var request = message.Params!.Deserialize<InlineCompletionParams>(lSPCommunication.JsonSerializerOptions)!;
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
