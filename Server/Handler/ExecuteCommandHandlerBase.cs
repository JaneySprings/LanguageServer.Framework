using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.ExecuteCommand;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class ExecuteCommandHandlerBase : IJsonHandler
{
    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("workspace/executeCommand", async (message, token) =>
        {
            var request = message.Params!.Deserialize<ExecuteCommandParams>(lspCommunication.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, lspCommunication.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }

    protected abstract Task<ExecuteCommandResponse> Handle(ExecuteCommandParams request, CancellationToken token);
}
