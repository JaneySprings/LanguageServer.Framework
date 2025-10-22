using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Progress;

/// <summary>
/// Work done progress cancel parameters.
/// </summary>
public class WorkDoneProgressCancelParams
{
    /// <summary>
    /// The token to be used to cancel the progress.
    /// </summary>
    [JsonPropertyName("token")]
    public required StringOrInt Token { get; set; }
}
