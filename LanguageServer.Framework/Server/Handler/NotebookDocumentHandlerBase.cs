using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.NotebookDocument;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class NotebookDocumentHandlerBase : IJsonHandler
{
    protected abstract Task Handle(DidOpenNotebookDocumentParams request, CancellationToken cancellationToken);

    protected abstract Task Handle(DidChangeNotebookDocumentParams request, CancellationToken cancellationToken);

    protected abstract Task Handle(DidCloseNotebookDocumentParams request, CancellationToken cancellationToken);

    protected abstract Task Handle(DidSaveNotebookDocumentParams request, CancellationToken cancellationToken);

    public virtual void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddNotificationHandler("notebookDocument/didClose", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidCloseNotebookDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });

        lSPCommunication.AddNotificationHandler("notebookDocument/didOpen", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidOpenNotebookDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });

        lSPCommunication.AddNotificationHandler("notebookDocument/didChange", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidChangeNotebookDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });

        lSPCommunication.AddNotificationHandler("notebookDocument/didSave", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidSaveNotebookDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
