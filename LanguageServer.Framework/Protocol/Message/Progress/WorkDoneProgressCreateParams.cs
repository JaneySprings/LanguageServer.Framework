using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Progress;

/// <summary>
/// Work done progress create parameters.
/// </summary>
public class WorkDoneProgressCreateParams
{
    /// <summary>
    /// The token to be used to report progress.
    /// </summary>
    [JsonPropertyName("token")]
    public required StringOrInt Token { get; set; }
}
