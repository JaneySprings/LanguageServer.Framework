using System.Text;
using LanguageServerType = EmmyLua.LanguageServer.Framework.Server.LanguageServer;

namespace EmmyLua.LanguageServer.Framework.Tests.TestBase;

/// <summary>
/// Mock language server for testing
/// </summary>
public class MockLanguageServer : LanguageServerType
{
    public MockLanguageServer() : base(new MemoryStream(), new MemoryStream())
    {
    }

    public static MockLanguageServer Create()
    {
        return new MockLanguageServer();
    }

    /// <summary>
    /// Get output stream content
    /// </summary>
    public string GetOutput()
    {
        if (Writer is { } writer)
        {
            var outputStream = typeof(LSPCommunicationBase)
                .GetField("_output", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(this) as MemoryStream;

            if (outputStream != null)
            {
                outputStream.Position = 0;
                using var reader = new StreamReader(outputStream, Encoding.UTF8, leaveOpen: true);
                return reader.ReadToEnd();
            }
        }

        return string.Empty;
    }
}