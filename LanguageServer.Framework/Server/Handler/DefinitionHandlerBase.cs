using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DefinitionHandlerBase : IJsonHandler
{
    protected abstract Task<DefinitionResponse?>
        Handle(DefinitionParams request, CancellationToken cancellationToken);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/definition", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DefinitionParams>(lspCommunication.JsonSerializerOptions)!;
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
