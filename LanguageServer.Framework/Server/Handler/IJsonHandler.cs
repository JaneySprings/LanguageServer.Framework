using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public interface IJsonHandler
{
    public void RegisterHandler(LSPCommunicationBase lSPCommunication);

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities);

    public void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities);
}
