using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceFiles;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class WorkspaceFilesHandlerBase : IJsonHandler
{
    protected abstract Task<WorkspaceEdit?> WillCreateFiles(CreateFilesParams request, CancellationToken token);

    protected abstract Task DidCreateFiles(CreateFilesParams request, CancellationToken token);

    protected abstract Task<WorkspaceEdit?> WillRenameFiles(RenameFilesParams request, CancellationToken token);

    protected abstract Task DidRenameFiles(RenameFilesParams request, CancellationToken token);

    protected abstract Task<WorkspaceEdit?> WillDeleteFiles(DeleteFilesParams request, CancellationToken token);

    protected abstract Task DidDeleteFiles(DeleteFilesParams request, CancellationToken token);

    public void RegisterHandler(LSPCommunicationBase lSPCommunication)
    {
        lSPCommunication.AddRequestHandler("workspace/willCreateFiles", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CreateFilesParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await WillCreateFiles(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddNotificationHandler("workspace/didCreateFiles", (message, token) =>
        {
            var request = message.Params!.Deserialize<CreateFilesParams>(lSPCommunication.JsonSerializerOptions)!;
            return DidCreateFiles(request, token);
        });

        lSPCommunication.AddRequestHandler("workspace/willRenameFiles", async (message, token) =>
        {
            var request = message.Params!.Deserialize<RenameFilesParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await WillRenameFiles(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddNotificationHandler("workspace/didRenameFiles", (message, token) =>
        {
            var request = message.Params!.Deserialize<RenameFilesParams>(lSPCommunication.JsonSerializerOptions)!;
            return DidRenameFiles(request, token);
        });

        lSPCommunication.AddRequestHandler("workspace/willDeleteFiles", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DeleteFilesParams>(lSPCommunication.JsonSerializerOptions)!;
            var r = await WillDeleteFiles(request, token);
            return JsonSerializer.SerializeToDocument(r, lSPCommunication.JsonSerializerOptions);
        });

        lSPCommunication.AddNotificationHandler("workspace/didDeleteFiles", (message, token) =>
        {
            var request = message.Params!.Deserialize<DeleteFilesParams>(lSPCommunication.JsonSerializerOptions)!;
            return DidDeleteFiles(request, token);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);

    public virtual void RegisterDynamicCapability(LSPCommunicationBase lSPCommunication, ClientCapabilities clientCapabilities)
    {
    }
}
