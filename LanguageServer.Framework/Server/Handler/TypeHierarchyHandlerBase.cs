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

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/prepareTypeHierarchy", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeHierarchyPrepareParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
        lSPCommunication.AddRequestHandler("typeHierarchy/supertypes", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeHierarchySupertypesParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
        lSPCommunication.AddRequestHandler("typeHierarchy/subtypes", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeHierarchySubtypesParams>(lSPCommunication.JsonSerializerOptions)!;
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
