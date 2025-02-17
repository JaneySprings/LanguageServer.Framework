using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class TextDocumentHandlerBase : IJsonHandler
{
    protected abstract Task Handle(DidOpenTextDocumentParams request, CancellationToken token);

    protected abstract Task Handle(DidChangeTextDocumentParams request, CancellationToken token);

    protected abstract Task Handle(DidCloseTextDocumentParams request, CancellationToken token);

    protected abstract Task Handle(WillSaveTextDocumentParams request, CancellationToken token);

    protected abstract Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request, CancellationToken token);

    public virtual void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddNotificationHandler("textDocument/didOpen",
            (notificationMessage, token) =>
            {
                var request = notificationMessage.Params!.Deserialize<DidOpenTextDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
                return Handle(request, token);
            });
        lSPCommunication.AddNotificationHandler("textDocument/didChange",
            (notificationMessage, token) =>
            {
                var request = notificationMessage.Params!.Deserialize<DidChangeTextDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
                return Handle(request, token);
            });
        lSPCommunication.AddNotificationHandler("textDocument/didClose",
            (notificationMessage, token) =>
            {
                var request = notificationMessage.Params!.Deserialize<DidCloseTextDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
                return Handle(request, token);
            });
        lSPCommunication.AddNotificationHandler("textDocument/willSave",
            (notificationMessage, token) =>
            {
                var request = notificationMessage.Params!.Deserialize<WillSaveTextDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
                return Handle(request, token);
            });
        lSPCommunication.AddRequestHandler("textDocument/willSaveWaitUntil",
            async (requestMessage, token) =>
            {
                var request = requestMessage.Params!.Deserialize<WillSaveTextDocumentParams>(lSPCommunication.JsonSerializerOptions)!;
                var r = await HandleRequest(request, token);
                return r == null ? null : JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
            });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
