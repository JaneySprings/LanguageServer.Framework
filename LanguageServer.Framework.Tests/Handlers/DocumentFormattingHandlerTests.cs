using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentFormatting;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DocumentFormattingHandlerTests : TestHandlerBase
{
    private class TestDocumentFormattingHandler : DocumentFormattingHandlerBase
    {
        protected override Task<DocumentFormattingResponse?> Handle(DocumentFormattingParams request,
            CancellationToken token)
        {
            var edits = new List<TextEdit>
            {
                new TextEdit
                {
                    Range = new DocumentRange(
                        new Position { Line = 0, Character = 0 },
                        new Position { Line = 0, Character = 10 }
                    ),
                    NewText = "formatted text"
                }
            };

            return Task.FromResult<DocumentFormattingResponse?>(new DocumentFormattingResponse(edits));
        }

        protected override Task<DocumentFormattingResponse?> Handle(DocumentRangesFormattingParams request,
            CancellationToken token)
        {
            return Task.FromResult<DocumentFormattingResponse?>(null);
        }

        protected override Task<DocumentFormattingResponse?> Handle(DocumentOnTypeFormattingParams request,
            CancellationToken token)
        {
            return Task.FromResult<DocumentFormattingResponse?>(null);
        }

        protected override Task<DocumentFormattingResponse?> Handle(DocumentRangeFormattingParams request,
            CancellationToken token)
        {
            var edits = new List<TextEdit>
            {
                new TextEdit
                {
                    Range = request.Range,
                    NewText = "formatted range"
                }
            };

            return Task.FromResult<DocumentFormattingResponse?>(new DocumentFormattingResponse(edits));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.DocumentFormattingProvider = true;
            serverCapabilities.DocumentRangeFormattingProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnFormattingEdits()
    {
        // Arrange
        var handler = new TestDocumentFormattingHandler();
        var request = new DocumentFormattingParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Options = new FormattingOptions
            {
                TabSize = 4,
                InsertSpaces = true
            }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<DocumentFormattingResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Edits.Should().HaveCount(1);
        response.Edits[0].NewText.Should().Be("formatted text");
    }

    [Fact]
    public async Task HandleRange_ShouldReturnRangeFormattingEdits()
    {
        // Arrange
        var handler = new TestDocumentFormattingHandler();
        var request = new DocumentRangeFormattingParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Range = new DocumentRange(
                new Position { Line = 5, Character = 0 },
                new Position { Line = 10, Character = 0 }
            ),
            Options = new FormattingOptions
            {
                TabSize = 2,
                InsertSpaces = true
            }
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(DocumentRangeFormattingParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [request, CancellationToken.None]) as Task<DocumentFormattingResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Edits.Should().HaveCount(1);
        response.Edits[0].NewText.Should().Be("formatted range");
    }

    [Fact]
    public void RegisterCapability_ShouldEnableFormattingProviders()
    {
        // Arrange
        var handler = new TestDocumentFormattingHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.DocumentFormattingProvider.Should().NotBeNull();
        serverCapabilities.DocumentRangeFormattingProvider.Should().NotBeNull();
    }
}