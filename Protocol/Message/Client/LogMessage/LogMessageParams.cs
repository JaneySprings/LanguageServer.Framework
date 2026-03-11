using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ShowMessage;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Client.LogMessage;

/// <summary>
/// The log message parameters.
/// </summary>
public class LogMessageParams
{
    /// <summary>
    /// The message type. See {@link MessageType}
    /// </summary>
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }

    /// <summary>
    /// The actual message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}
