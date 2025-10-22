using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ShowMessage;

/// <summary>
/// Show message request client capabilities.
/// </summary>
public class ShowMessageRequestClientCapabilities
{
    /// <summary>
    /// Capabilities specific to the `MessageActionItem` type.
    /// </summary>
    [JsonPropertyName("messageActionItem")]
    public MessageActionItemCapabilities? MessageActionItem { get; set; }
}

/// <summary>
/// Capabilities specific to the `MessageActionItem` type.
/// </summary>
public class MessageActionItemCapabilities
{
    /// <summary>
    /// Whether the client supports additional attributes which
    /// are preserved and sent back to the server in the
    /// request's response.
    /// </summary>
    [JsonPropertyName("additionalPropertiesSupport")]
    public bool? AdditionalPropertiesSupport { get; set; }
}
