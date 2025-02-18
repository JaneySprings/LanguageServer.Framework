using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class CodeActionHandlerBase : IJsonHandler
{
    protected abstract Task<CodeActionResponse> Handle(CodeActionParams request, CancellationToken token);

    protected abstract Task<CodeAction> Resolve(CodeAction request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/codeAction", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CodeActionParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("codeAction/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CodeAction>(lspCommunication.JsonSerializerOptions)!;
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
