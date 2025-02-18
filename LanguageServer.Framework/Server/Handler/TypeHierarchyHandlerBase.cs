using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeHierarchy;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class TypeHierarchyHandlerBase : IJsonHandler
{
    protected abstract Task<TypeHierarchyResponse?> Handle(TypeHierarchyPrepareParams typeHierarchyPrepareParams,
        CancellationToken cancellationToken);

    protected abstract Task<TypeHierarchyResponse?> Handle(TypeHierarchySupertypesParams typeHierarchySupertypesParams,
        CancellationToken cancellationToken);

    protected abstract Task<TypeHierarchyResponse?> Handle(TypeHierarchySubtypesParams typeHierarchySubtypesParams,
        CancellationToken cancellationToken);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/prepareTypeHierarchy", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeHierarchyPrepareParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
        lspCommunication.AddRequestHandler("typeHierarchy/supertypes", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeHierarchySupertypesParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
        lspCommunication.AddRequestHandler("typeHierarchy/subtypes", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeHierarchySubtypesParams>(lspCommunication.JsonSerializerOptions)!;
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
