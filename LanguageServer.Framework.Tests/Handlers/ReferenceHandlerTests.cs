using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class ReferenceHandlerTests : TestHandlerBase
{
    private class TestReferenceHandler : ReferenceHandlerBase
    {
        protected override Task<ReferenceResponse?> Handle(ReferenceParams request, CancellationToken token)
        {
            var locations = new List<Location>
            {
                new()
                {
                    Uri = "file:///ref1.txt",
                    Range = new DocumentRange(
                        new Position { Line = 5, Character = 10 },
                        new Position { Line = 5, Character = 20 }
                    )
                },
                new()
                {
                    Uri = "file:///ref2.txt",
                    Range = new DocumentRange(
                        new Position { Line = 15, Character = 5 },
                        new Position { Line = 15, Character = 15 }
                    )
                }
            };

            return Task.FromResult<ReferenceResponse?>(new ReferenceResponse(locations));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.ReferencesProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnReferenceLocations()
    {
        // Arrange
        var handler = new TestReferenceHandler();
        var request = new ReferenceParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Position = new Position { Line = 10, Character = 15 },
            Context = new ReferenceContext { IncludeDeclaration = true }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<ReferenceResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().HaveCount(2);
        response.Result[0].Uri.Uri.AbsoluteUri.Should().Be("file:///ref1.txt");
        response.Result[1].Uri.Uri.AbsoluteUri.Should().Be("file:///ref2.txt");
    }

    [Fact]
    public void RegisterCapability_ShouldEnableReferencesProvider()
    {
        // Arrange
        var handler = new TestReferenceHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.ReferencesProvider.Should().NotBeNull();
    }
}