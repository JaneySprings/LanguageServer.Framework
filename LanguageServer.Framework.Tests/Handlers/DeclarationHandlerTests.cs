using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DeclarationHandlerTests : TestHandlerBase
{
    private class TestDeclarationHandler : DeclarationHandlerBase
    {
        protected override Task<DeclarationResponse?> Handle(DeclarationParams request, CancellationToken token)
        {
            var locations = new List<Location>
            {
                new Location
                {
                    Uri = "file:///declarations.txt",
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 2, Character = 0 },
                        End = new Position { Line = 2, Character = 25 }
                    }
                }
            };

            return Task.FromResult<DeclarationResponse?>(new DeclarationResponse(locations));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.DeclarationProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnDeclarationLocations()
    {
        // Arrange
        var handler = new TestDeclarationHandler();
        var request = new DeclarationParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Position = new Position { Line = 10, Character = 15 }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<DeclarationResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Locations.Should().HaveCount(1);
        response.Locations[0].Uri.Should().Be("file:///declarations.txt");
        response.Locations[0].Range.Start.Line.Should().Be(2);
    }

    [Fact]
    public void RegisterCapability_ShouldEnableDeclarationProvider()
    {
        // Arrange
        var handler = new TestDeclarationHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.DeclarationProvider.Should().NotBeNull();
    }
}
