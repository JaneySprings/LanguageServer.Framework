using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace EmmyLua.LanguageServer.Framework.Tests.TestBase;

/// <summary>
/// Base class for handler tests
/// </summary>
public abstract class TestHandlerBase
{
    protected MockLanguageServer Server { get; }

    protected TestHandlerBase()
    {
        Server = MockLanguageServer.Create();
    }

    /// <summary>
    /// Add handler to server
    /// </summary>
    protected void AddHandler(IJsonHandler handler)
    {
        Server.AddHandler(handler);
    }
}