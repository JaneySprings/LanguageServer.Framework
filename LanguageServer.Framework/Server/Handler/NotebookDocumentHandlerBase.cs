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

    public virtual void RegisterHandler(LSPCommunicationBase lspCommunication)
    {
        lspCommunication.AddNotificationHandler("notebookDocument/didClose", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidCloseNotebookDocumentParams>(lspCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });

        lspCommunication.AddNotificationHandler("notebookDocument/didOpen", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidOpenNotebookDocumentParams>(lspCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });

        lspCommunication.AddNotificationHandler("notebookDocument/didChange", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidChangeNotebookDocumentParams>(lspCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });

        lspCommunication.AddNotificationHandler("notebookDocument/didSave", (message, token) =>
        {
            var request = message.Params!.Deserialize<DidSaveNotebookDocumentParams>(lspCommunication.JsonSerializerOptions)!;
            return Handle(request, token);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
