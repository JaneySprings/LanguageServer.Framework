using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class CodeActionHandlerBase : IJsonHandler
{
    protected abstract Task<CodeActionResponse> Handle(CodeActionParams request, CancellationToken token);

    protected abstract Task<CodeAction> Resolve(CodeAction request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/codeAction", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CodeActionParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("codeAction/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CodeAction>(lSPCommunication.JsonSerializerOptions)!;
            await Resolve(request, token);
            return JsonSerializer.SerializeToDocument(request, lSPCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
