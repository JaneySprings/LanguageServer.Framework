using System.Text;
using LanguageServerType = EmmyLua.LanguageServer.Framework.Server.LanguageServer;

namespace EmmyLua.LanguageServer.Framework.Tests.TestBase;

/// <summary>
/// 用于测试的模拟语言服务器
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
    /// 获取输出流内容
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
