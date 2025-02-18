using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.LinkedEditingRange;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class LinkedEditingRangeHandlerBase : IJsonHandler
{
    protected abstract Task<LinkedEditingRanges?> Handle(LinkedEditingRangeParams request,
        CancellationToken cancellationToken);

    public void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddRequestHandler("textDocument/linkedEditingRange", async (message, token) =>
        {
            var request = message.Params!.Deserialize<LinkedEditingRangeParams>(lspCommunication.JsonSerializerOptions)!;
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
