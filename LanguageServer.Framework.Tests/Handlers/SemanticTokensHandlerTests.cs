using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class SemanticTokensHandlerTests : TestHandlerBase
{
    private class TestSemanticTokensHandler : SemanticTokensHandlerBase
    {
        protected override Task<SemanticTokens?> Handle(SemanticTokensParams request, CancellationToken token)
        {
            // Semantic tokens format: [deltaLine, deltaStart, length, tokenType, tokenModifiers]
            var data = new List<uint>
            {
                0, 0, 5, 0, 0, // Token 1
                0, 6, 10, 1, 0, // Token 2
                1, 0, 8, 2, 1 // Token 3
            };

            return Task.FromResult<SemanticTokens?>(new SemanticTokens
            {
                Data = data
            });
        }

        protected override Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams request,
            CancellationToken token)
        {
            return Task.FromResult<SemanticTokensDeltaResponse?>(new SemanticTokensDeltaResponse(new SemanticTokens()));
        }

        protected override Task<SemanticTokens?> Handle(SemanticTokensRangeParams request, CancellationToken token)
        {
            var data = new List<uint>
            {
                0, 0, 5, 0, 0
            };

            return Task.FromResult<SemanticTokens?>(new SemanticTokens
            {
                Data = data
            });
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.SemanticTokensProvider = new Protocol.Capabilities.Server.Options.SemanticTokensOptions
            {
                Legend = new SemanticTokensLegend
                {
                    TokenTypes = ["class", "function", "variable"],
                    TokenModifiers = ["declaration", "readonly"]
                },
                Full = true,
                Range = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnSemanticTokens()
    {
        // Arrange
        var handler = new TestSemanticTokensHandler();
        var request = new SemanticTokensParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt")
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(SemanticTokensParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [request, CancellationToken.None]) as Task<SemanticTokens?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeEmpty();
        response.Data.Count.Should().Be(15); // 3 tokens * 5 values each
    }

    [Fact]
    public async Task HandleRange_ShouldReturnSemanticTokensForRange()
    {
        // Arrange
        var handler = new TestSemanticTokensHandler();
        var request = new SemanticTokensRangeParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Range = new DocumentRange(
                new Position { Line = 0, Character = 0 },
                new Position { Line = 5, Character = 0 }
            )
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(SemanticTokensRangeParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [request, CancellationToken.None]) as Task<SemanticTokens?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Data.Should().NotBeEmpty();
        response.Data.Count.Should().Be(5); // 1 token * 5 values
    }

    [Fact]
    public void RegisterCapability_ShouldSetSemanticTokensProvider()
    {
        // Arrange
        var handler = new TestSemanticTokensHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.SemanticTokensProvider.Should().NotBeNull();
        var options = serverCapabilities.SemanticTokensProvider;
        options.Should().NotBeNull();
        options!.Legend.TokenTypes.Should().Contain("class");
        options.Full!.BoolValue.Should().BeTrue();
        options.Range.Should().BeTrue();
    }
}