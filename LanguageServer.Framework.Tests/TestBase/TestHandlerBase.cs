using EmmyLua.LanguageServer.Framework.Server;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace EmmyLua.LanguageServer.Framework.Tests.TestBase;

/// <summary>
/// Handler 测试基类
/// </summary>
public abstract class TestHandlerBase
{
    protected MockLanguageServer Server { get; }

    protected TestHandlerBase()
    {
        Server = MockLanguageServer.Create();
    }

    /// <summary>
    /// 添加 Handler 到服务器
    /// </summary>
    protected void AddHandler(IJsonHandler handler)
    {
        Server.AddHandler(handler);
    }
}
