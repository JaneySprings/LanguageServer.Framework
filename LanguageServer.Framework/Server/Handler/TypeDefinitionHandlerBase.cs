using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeDefinition;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class TypeDefinitionHandlerBase : IJsonHandler
{
    protected abstract Task<TypeDefinitionResponse?>
        Handle(TypeDefinitionParams request, CancellationToken cancellationToken);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/typeDefinition", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeDefinitionParams>(lSPCommunication.JsonSerializerOptions)!;
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
