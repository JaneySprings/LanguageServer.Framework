using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Telemetry;

/// <summary>
/// The telemetry event parameters.
/// </summary>
public class TelemetryEventParams
{
    /// <summary>
    /// The telemetry data.
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
