using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentHighlight;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DocumentHighlightHandlerTests : TestHandlerBase
{
    private class TestDocumentHighlightHandler : DocumentHighlightHandlerBase
    {
        protected override Task<DocumentHighlightResponse> Handle(DocumentHighlightParams request,
            CancellationToken token)
        {
            var highlights = new List<DocumentHighlight>
            {
                new()
                {
                    Range = new DocumentRange(
                        new Position { Line = 5, Character = 10 },
                        new Position { Line = 5, Character = 20 }
                    ),
                    Kind = DocumentHighlightKind.Write
                },
                new()
                {
                    Range = new DocumentRange(
                        new Position { Line = 10, Character = 5 },
                        new Position { Line = 10, Character = 15 }
                    ),
                    Kind = DocumentHighlightKind.Read
                }
            };

            return Task.FromResult(new DocumentHighlightResponse(highlights));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.DocumentHighlightProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnDocumentHighlights()
    {
        // Arrange
        var handler = new TestDocumentHighlightHandler();
        var request = new DocumentHighlightParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Position = new Position { Line = 5, Character = 15 }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<DocumentHighlightResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Highlights.Should().HaveCount(2);
        response.Highlights[0].Kind.Should().Be(DocumentHighlightKind.Write);
        response.Highlights[1].Kind.Should().Be(DocumentHighlightKind.Read);
    }

    [Fact]
    public void RegisterCapability_ShouldEnableDocumentHighlightProvider()
    {
        // Arrange
        var handler = new TestDocumentHighlightHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.DocumentHighlightProvider.Should().NotBeNull();
    }
}