using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentLink;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DocumentLinkHandlerTests : TestHandlerBase
{
    private class TestDocumentLinkHandler : DocumentLinkHandlerBase
    {
        protected override Task<DocumentLinkResponse> Handle(DocumentLinkParams request, CancellationToken token)
        {
            var links = new List<DocumentLink>
            {
                new()
                {
                    Range = new DocumentRange(
                        new Position { Line = 0, Character = 0 },
                        new Position { Line = 0, Character = 20 }
                    ),
                    Target = "https://example.com",
                    Tooltip = "Example link"
                }
            };

            return Task.FromResult(new DocumentLinkResponse(links));
        }

        protected override Task<DocumentLink> Resolve(DocumentLink documentLink, CancellationToken token)
        {
            documentLink.Tooltip += " (resolved)";
            return Task.FromResult(documentLink);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.DocumentLinkProvider = new Protocol.Capabilities.Server.Options.DocumentLinkOptions
            {
                ResolveProvider = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnDocumentLinks()
    {
        // Arrange
        var handler = new TestDocumentLinkHandler();
        var request = new DocumentLinkParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt")
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<DocumentLinkResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.DocumentLinks.Should().HaveCount(1);
        response.DocumentLinks[0].Target!.Value.Uri.AbsoluteUri.Should().Be("https://example.com/");
        response.DocumentLinks[0].Tooltip.Should().Be("Example link");
    }

    [Fact]
    public async Task Resolve_ShouldUpdateDocumentLink()
    {
        // Arrange
        var handler = new TestDocumentLinkHandler();
        var link = new DocumentLink
        {
            Range = new DocumentRange(
                new Position { Line = 0, Character = 0 },
                new Position { Line = 0, Character = 10 }
            ),
            Tooltip = "Test link"
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [link, CancellationToken.None]);

        var resolved = await (task as Task<DocumentLink>)!;

        // Assert
        resolved.Should().NotBeNull();
        resolved.Tooltip.Should().Contain("resolved");
    }

    [Fact]
    public void RegisterCapability_ShouldSetDocumentLinkProvider()
    {
        // Arrange
        var handler = new TestDocumentLinkHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.DocumentLinkProvider.Should().NotBeNull();
        serverCapabilities.DocumentLinkProvider!.ResolveProvider.Should().BeTrue();
    }
}