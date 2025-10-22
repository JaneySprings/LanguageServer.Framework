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
using LanguageServerType = EmmyLua.LanguageServer.Framework.Server.LanguageServer;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class TextDocumentHandlerTests : TestHandlerBase
{
    private class TestTextDocumentHandler : TextDocumentHandlerBase
    {
        private readonly Dictionary<string, string> _documents = new();

        public TestTextDocumentHandler(LanguageServerType server) : base(server)
        {
        }

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

        protected override Task<List<TextEdit>?> HandleRequest(WillSaveTextDocumentParams request, CancellationToken token)
        {
            return Task.FromResult<List<TextEdit>?>(null);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.TextDocumentSync = new Protocol.Capabilities.Server.Options.TextDocumentSyncOptions
            {
                OpenClose = true,
                Change = Protocol.Model.TextDocumentSyncKind.Incremental,
                Save = new Protocol.Capabilities.Server.Options.SaveOptions
                {
                    IncludeText = true
                }
            };
        }

        public string? GetDocumentContent(string uri)
        {
            return _documents.TryGetValue(uri, out var content) ? content : null;
        }
    }

    [Fact]
    public async Task DidOpen_ShouldStoreDocument()
    {
        // Arrange
        var handler = new TestTextDocumentHandler(Server);
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
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(DidOpenTextDocumentParams), typeof(CancellationToken) }, null)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task;

        await result!;

        // Assert
        handler.GetDocumentContent("file:///test.txt").Should().Be("Hello World");
    }

    [Fact]
    public async Task DidChange_ShouldUpdateDocument()
    {
        // Arrange
        var handler = new TestTextDocumentHandler(Server);
        
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
            .GetMethod("DidOpen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { openRequest, CancellationToken.None }) as Task)!;

        // Now change it
        var changeRequest = new DidChangeTextDocumentParams
        {
            TextDocument = new VersionedTextDocumentIdentifier
            {
                Uri = "file:///test.txt",
                Version = 2
            },
            ContentChanges = new List<TextDocumentContentChangeEvent>
            {
                new TextDocumentContentChangeEvent
                {
                    Text = "Updated text"
                }
            }
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(DidChangeTextDocumentParams), typeof(CancellationToken) }, null)!
            .Invoke(handler, new object[] { changeRequest, CancellationToken.None }) as Task;

        await result!;

        // Assert
        handler.GetDocumentContent("file:///test.txt").Should().Be("Updated text");
    }

    [Fact]
    public async Task DidClose_ShouldRemoveDocument()
    {
        // Arrange
        var handler = new TestTextDocumentHandler(Server);
        
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
            .GetMethod("DidOpen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { openRequest, CancellationToken.None }) as Task)!;

        // Now close it
        var closeRequest = new DidCloseTextDocumentParams
        {
            TextDocument = new TextDocumentIdentifier
            {
                Uri = "file:///test.txt"
            }
        };

        // Act
        var result = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(DidCloseTextDocumentParams), typeof(CancellationToken) }, null)!
            .Invoke(handler, new object[] { closeRequest, CancellationToken.None }) as Task;

        await result!;

        // Assert
        handler.GetDocumentContent("file:///test.txt").Should().BeNull();
    }

    [Fact]
    public void RegisterCapability_ShouldSetTextDocumentSync()
    {
        // Arrange
        var handler = new TestTextDocumentHandler(Server);
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.TextDocumentSync.Should().NotBeNull();
        var options = serverCapabilities.TextDocumentSync as Protocol.Capabilities.Server.Options.TextDocumentSyncOptions;
        options.Should().NotBeNull();
        options!.OpenClose.Should().BeTrue();
        options.Change.Should().Be(Protocol.Model.TextDocumentSyncKind.Incremental);
    }
}
