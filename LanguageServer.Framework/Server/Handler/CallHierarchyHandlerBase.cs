using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class CallHierarchyHandlerBase : IJsonHandler
{
    protected abstract Task<CallHierarchyPrepareResponse?> CallHierarchyPrepare(CallHierarchyPrepareParams request,
        CancellationToken token);

    protected abstract Task<CallHierarchyIncomingCallsResponse> CallHierarchyIncomingCalls(
        CallHierarchyIncomingCallsParams request, CancellationToken token);

    protected abstract Task<CallHierarchyOutgoingCallsResponse> CallHierarchyOutgoingCalls(
        CallHierarchyOutgoingCallsParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("textDocument/prepareCallHierarchy", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CallHierarchyPrepareParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await CallHierarchyPrepare(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("callHierarchy/incomingCalls", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CallHierarchyIncomingCallsParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await CallHierarchyIncomingCalls(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddRequestHandler("callHierarchy/outgoingCalls", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CallHierarchyOutgoingCallsParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await CallHierarchyOutgoingCalls(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
