using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Configuration;

public class DidChangeConfigurationParams
{
    /**
	 * The actual changed settings
	 */
    [JsonPropertyName("settings")]
    public LSPAny Settings { get; set; } = null!;
}