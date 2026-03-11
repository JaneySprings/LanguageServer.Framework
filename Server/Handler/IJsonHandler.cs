using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public interface IJsonHandler
{
    void RegisterHandler(LSPCommunicationBase lspCommunication);

    void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities);

    void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities);
}
