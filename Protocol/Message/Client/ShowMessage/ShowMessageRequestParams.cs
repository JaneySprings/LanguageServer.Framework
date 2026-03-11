using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ShowMessage;

/// <summary>
/// Show message request parameters.
/// </summary>
public class ShowMessageRequestParams
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

    /// <summary>
    /// The message action items to present.
    /// </summary>
    [JsonPropertyName("actions")]
    public List<MessageActionItem>? Actions { get; set; }
}

/// <summary>
/// A message action item.
/// </summary>
public class MessageActionItem
{
    /// <summary>
    /// A short title like 'Retry', 'Open Log' etc.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = null!;
}
