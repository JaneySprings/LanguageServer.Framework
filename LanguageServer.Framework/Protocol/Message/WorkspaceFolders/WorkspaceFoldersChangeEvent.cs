using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceFolders;

public class WorkspaceFoldersChangeEvent
{
    /**
     * The array of added workspace folders
     */
    [JsonPropertyName("added")]
    public List<Model.WorkspaceFolder> Added { get; set; } = null!;

    /**
     * The array of the removed workspace folders
     */
    [JsonPropertyName("removed")]
    public List<Model.WorkspaceFolder> Removed { get; set; } = null!;
}
