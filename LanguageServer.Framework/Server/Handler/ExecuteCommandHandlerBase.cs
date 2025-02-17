using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.ExecuteCommand;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class ExecuteCommandHandlerBase : IJsonHandler
{
    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("workspace/executeCommand", async (message, token) =>
        {
            var request = message.Params!.Deserialize<ExecuteCommandParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }

    protected abstract Task<ExecuteCommandResponse> Handle(ExecuteCommandParams request, CancellationToken token);
}
