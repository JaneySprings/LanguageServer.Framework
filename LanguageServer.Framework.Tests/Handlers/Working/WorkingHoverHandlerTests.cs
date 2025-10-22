using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Markup;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers.Working;

/// <summary>
/// 可工作的 HoverHandler 测试示例
/// </summary>
public class WorkingHoverHandlerTests
{
    private class TestHoverHandler : HoverHandlerBase
    {
        protected override Task<HoverResponse?> Handle(HoverParams request, CancellationToken token)
        {
            return Task.FromResult<HoverResponse?>(new HoverResponse
            {
                Contents = new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = "## Test Function\n\nThis is a test function."
                },
                Range = new DocumentRange(
                    new Position { Line = 0, Character = 0 },
                    new Position { Line = 0, Character = 10 }
                )
            });
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.HoverProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnHoverInformation()
    {
        // Arrange
        var handler = new TestHoverHandler();
        var request = new HoverParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Position = new Position { Line = 5, Character = 10 }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = method.Invoke(handler, [request, CancellationToken.None]);
        var response = await (task as Task<HoverResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Contents.Should().NotBeNull();
        response.Contents.Value.Should().Contain("Test Function");
        response.Range.Should().NotBeNull();
    }

    [Fact]
    public void RegisterCapability_ShouldEnableHoverProvider()
    {
        // Arrange
        var handler = new TestHoverHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.HoverProvider.Should().NotBeNull();
    }
}
