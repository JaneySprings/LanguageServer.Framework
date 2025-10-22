using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class TextDocumentHandlerTests : TestHandlerBase
{
    private class TestTextDocumentHandler : TextDocumentHandlerBase
    {
        private readonly Dictionary<DocumentUri, string> _documents = new();

        protected override Task Handle(DidOpenTextDocumentParams request, CancellationToken token)
        {
            _documents[request.TextDocument.Uri] = request.TextDocument.Text;
            return Task.CompletedTask;
        }

        protected override Task Handle(DidChangeTextDocumentParams request, CancellationToken token)
        {
            if (request.ContentChanges.Count > 0)
            {
                _documents[request.TextDocument.Uri] = request.ContentChanges[0].Text;
            }

            return Task.CompletedTask;
        }

        protected override Task Handle(DidCloseTextDocumentParams request, CancellationToken token)
        {
            _documents.Remove(request.TextDocument.Uri);
            return Task.CompletedTask;
        }

        protected override Task Handle(WillSaveTextDocumentParams request, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request,
            CancellationToken token)
        {
            return Task.FromResult<List<TextEdit>?>(null);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.TextDocumentSync = new Protocol.Capabilities.Server.Options.TextDocumentSyncOptions
            {
                OpenClose = true,
                Change = TextDocumentSyncKind.Incremental,
                Save = new Protocol.Capabilities.Server.Options.SaveOptions
                {
                    IncludeText = true
                }
            };
        }

        public string? GetDocumentContent(DocumentUri uri)
        {
            return _documents.GetValueOrDefault(uri);
        }
    }

    [Fact]
    public async Task DidOpen_ShouldStoreDocument()
    {
        // Arrange
        var handler = new TestTextDocumentHandler();
        var request = new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = "file:///test.txt",
                LanguageId = "plaintext",
                Version = 1,
                Text = "Hello World"
            }
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(DidOpenTextDocumentParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [request, CancellationToken.None]) as Task;

        await result!;

        // Assert
        handler.GetDocumentContent("file:///test.txt").Should().Be("Hello World");
    }

    [Fact]
    public async Task DidChange_ShouldUpdateDocument()
    {
        // Arrange
        var handler = new TestTextDocumentHandler();

        // First open the document
        var openRequest = new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = "file:///test.txt",
                LanguageId = "plaintext",
                Version = 1,
                Text = "Original text"
            }
        };

        await (handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(DidOpenTextDocumentParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [openRequest, CancellationToken.None]) as Task)!;

        // Now change it
        var changeRequest = new DidChangeTextDocumentParams
        {
            TextDocument = new VersionedTextDocumentIdentifier("file:///test.txt", 2),
            ContentChanges =
            [
                new TextDocumentContentChangeEvent
                {
                    Text = "Updated text"
                }
            ]
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(DidChangeTextDocumentParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [changeRequest, CancellationToken.None]) as Task;

        await result!;

        // Assert
        handler.GetDocumentContent("file:///test.txt").Should().Be("Updated text");
    }

    [Fact]
    public async Task DidClose_ShouldRemoveDocument()
    {
        // Arrange
        var handler = new TestTextDocumentHandler();

        // First open the document
        var openRequest = new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = "file:///test.txt",
                LanguageId = "plaintext",
                Version = 1,
                Text = "Test content"
            }
        };

        await (handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(DidOpenTextDocumentParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [openRequest, CancellationToken.None]) as Task)!;

        // Now close it
        var closeRequest = new DidCloseTextDocumentParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt")
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(DidCloseTextDocumentParams), typeof(CancellationToken)], null)!
            .Invoke(handler, [closeRequest, CancellationToken.None]) as Task;

        await result!;

        // Assert
        handler.GetDocumentContent("file:///test.txt").Should().BeNull();
    }

    [Fact]
    public void RegisterCapability_ShouldSetTextDocumentSync()
    {
        // Arrange
        var handler = new TestTextDocumentHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.TextDocumentSync.Should().NotBeNull();
        var options = serverCapabilities.TextDocumentSync;
        options.Should().NotBeNull();
        options!.Value!.OpenClose.Should().BeTrue();
        options.Value.Change.Should().Be(TextDocumentSyncKind.Incremental);
    }
}