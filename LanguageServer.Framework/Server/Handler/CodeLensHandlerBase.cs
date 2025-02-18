using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeLens;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class CodeLensHandlerBase : IJsonHandler
{
    protected abstract Task<CodeLensResponse> Handle(CodeLensParams request, CancellationToken token);

    protected abstract Task<CodeLens> Resolve(CodeLens request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/codeLens", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CodeLensParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });

        lspCommunication.AddRequestHandler("codeLens/resolve", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CodeLens>(lspCommunication.JsonSerializerOptions)!;
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
