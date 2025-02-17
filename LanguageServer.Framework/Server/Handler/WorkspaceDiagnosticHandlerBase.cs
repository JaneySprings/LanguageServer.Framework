using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceDiagnostic;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class WorkspaceDiagnosticHandlerBase : IJsonHandler
{
    protected abstract Task<WorkspaceDiagnosticReport> Handle(WorkspaceDiagnosticParams request,
        CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("workspace/diagnostic", async (message, token) =>
        {
            var request = message.Params!.Deserialize<WorkspaceDiagnosticParams>(lSPCommunication.JsonSerializerOptions)!;
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
