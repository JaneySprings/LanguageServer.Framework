using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class InlayHintHandlerTests : TestHandlerBase
{
    private class TestInlayHintHandler : InlayHintHandlerBase
    {
        protected override Task<InlayHintResponse?> Handle(InlayHintParams request, CancellationToken token)
        {
            var hints = new List<InlayHint>
            {
                new InlayHint
                {
                    Position = new Position { Line = 5, Character = 20 },
                    Label = ": string",
                    Kind = InlayHintKind.Type,
                    PaddingLeft = false,
                    PaddingRight = false
                },
                new InlayHint
                {
                    Position = new Position { Line = 10, Character = 15 },
                    Label = "value:",
                    Kind = InlayHintKind.Parameter,
                    PaddingRight = true
                }
            };

            return Task.FromResult<InlayHintResponse?>(new InlayHintResponse(hints));
        }

        protected override Task<InlayHint> Resolve(InlayHint inlayHint, CancellationToken token)
        {
            inlayHint.Tooltip = "Resolved tooltip";
            return Task.FromResult(inlayHint);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.InlayHintProvider = new InlayHintsOptions
            {
                ResolveProvider = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnInlayHints()
    {
        // Arrange
        var handler = new TestInlayHintHandler();
        var request = new InlayHintParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Range = new DocumentRange(
                new Position { Line = 0, Character = 0 },
                new Position { Line = 20, Character = 0 }
            )
        };

        // Act
        var method = handler.GetType()

            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<InlayHintResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.InlayHints.Should().HaveCount(2);
        response.InlayHints[0].Kind.Should().Be(InlayHintKind.Type);
        response.InlayHints[1].Kind.Should().Be(InlayHintKind.Parameter);
    }

    [Fact]
    public async Task Resolve_ShouldAddTooltip()
    {
        // Arrange
        var handler = new TestInlayHintHandler();
        var hint = new InlayHint
        {
            Position = new Position { Line = 5, Character = 10 },
            Label = "test"
        };

        // Act
        var method = handler.GetType()

            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [hint, CancellationToken.None]);

        var resolved = await (task as Task<InlayHint>)!;

        // Assert
        resolved.Should().NotBeNull();
        resolved.Tooltip!.StringValue.Should().Be("Resolved tooltip");
    }

    [Fact]
    public void RegisterCapability_ShouldSetInlayHintProvider()
    {
        // Arrange
        var handler = new TestInlayHintHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.InlayHintProvider.Should().NotBeNull();
        var options = serverCapabilities.InlayHintProvider!.Value;
        options.Should().NotBeNull();
        options!.ResolveProvider.Should().BeTrue();
    }
}
