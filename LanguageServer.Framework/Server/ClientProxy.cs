using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ApplyWorkspaceEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.LogMessage;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.PublishDiagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Registration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ShowMessage;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Telemetry;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Configuration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Progress;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;

namespace EmmyLua.LanguageServer.Framework.Server;

public class ClientProxy(LanguageServer server)
{
    public Task DynamicRegisterCapability(RegistrationParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        server.SendRequestNoWait("client/registerCapability", document);
        return Task.CompletedTask;
    }

    public Task DynamicUnregisterCapability(UnregistrationParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        server.SendRequestNoWait("client/unregisterCapability", document);
        return Task.CompletedTask;
    }

    public async Task<ApplyWorkspaceEditResult> ApplyEdit(ApplyWorkspaceEditParams @params, CancellationToken token)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var response = await server.SendRequest("workspace/applyEdit", document, token);
        return response!.Deserialize<ApplyWorkspaceEditResult>(server.JsonSerializerOptions)!;
    }

    public async Task<List<LSPAny>> GetConfiguration(ConfigurationParams @params, CancellationToken token)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var response = await server.SendRequest("workspace/configuration", document, token);
        return response!.Deserialize<List<LSPAny>>(server.JsonSerializerOptions)!;
    }

    public Task ShowMessage(ShowMessageParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("window/showMessage", document);
        return server.SendNotification(notification);
    }

    public async Task<MessageActionItem?> ShowMessageRequest(ShowMessageRequestParams @params, CancellationToken token)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var response = await server.SendRequest("window/showMessageRequest", document, token);
        return response?.Deserialize<MessageActionItem>(server.JsonSerializerOptions);
    }

    public Task LogMessage(LogMessageParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("window/logMessage", document);
        return server.SendNotification(notification);
    }

    public Task LogMessage(MessageType type, string message)
    {
        return LogMessage(new LogMessageParams { Type = type, Message = message });
    }

    public Task LogError(string message)
    {
        return LogMessage(MessageType.Error, message);
    }

    public Task LogWarning(string message)
    {
        return LogMessage(MessageType.Warning, message);
    }

    public Task LogInfo(string message)
    {
        return LogMessage(MessageType.Info, message);
    }

    public Task LogDebug(string message)
    {
        return LogMessage(MessageType.Debug, message);
    }

    public Task TelemetryEvent(object? data)
    {
        var @params = new TelemetryEventParams { Data = data };
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("telemetry/event", document);
        return server.SendNotification(notification);
    }

    public Task PublishDiagnostics(PublishDiagnosticsParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("textDocument/publishDiagnostics", document);
        return server.SendNotification(notification);
    }

    public async Task RefreshWorkspaceTokens()
    {
        await server.SendRequest("workspace/semanticTokens/refresh", null, CancellationToken.None);
    }

    public async Task RefreshInlineValues()
    {
        await server.SendRequest("workspace/inlineValue/refresh", null, CancellationToken.None);
    }

    public async Task RefreshInlayHint()
    {
        await server.SendRequest("workspace/inlayHint/refresh", null, CancellationToken.None);
    }

    public async Task RefreshDiagnostics()
    {
        await server.SendRequest("workspace/diagnostic/refresh", null, CancellationToken.None);
    }

    public async Task<JsonDocument?> SendRequest(string method, JsonDocument document, CancellationToken token)
    {
        var response = server.SendRequest(method, document, token);
        return await response;
    }

    // Work Done Progress Support

    /// <summary>
    /// Create a work done progress token and request the client to show progress UI.
    /// </summary>
    public async Task<StringOrInt> CreateWorkDoneProgress(StringOrInt token,
        CancellationToken cancellationToken = default)
    {
        var @params = new WorkDoneProgressCreateParams { Token = token };
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        await server.SendRequest("window/workDoneProgress/create", document, cancellationToken);
        return token;
    }

    /// <summary>
    /// Report progress begin.
    /// </summary>
    public Task ReportProgress(StringOrInt token, WorkDoneProgressBegin value)
    {
        var @params = new ProgressParams
        {
            Token = token.StringValue ?? token.IntValue.ToString(),
            Value = value
        };
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("$/progress", document);
        return server.SendNotification(notification);
    }

    /// <summary>
    /// Report progress update.
    /// </summary>
    public Task ReportProgress(StringOrInt token, WorkDoneProgressReport value)
    {
        var @params = new ProgressParams
        {
            Token = token.StringValue ?? token.IntValue.ToString(),
            Value = value
        };
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("$/progress", document);
        return server.SendNotification(notification);
    }

    /// <summary>
    /// Report progress end.
    /// </summary>
    public Task ReportProgress(StringOrInt token, WorkDoneProgressEnd value)
    {
        var @params = new ProgressParams
        {
            Token = token.StringValue ?? token.IntValue.ToString(),
            Value = value
        };
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("$/progress", document);
        return server.SendNotification(notification);
    }
}
