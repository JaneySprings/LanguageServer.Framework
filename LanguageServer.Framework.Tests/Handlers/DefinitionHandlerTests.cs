using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DefinitionHandlerTests : TestHandlerBase
{
    private class TestDefinitionHandler : DefinitionHandlerBase
    {
        protected override Task<DefinitionResponse?> Handle(DefinitionParams request, CancellationToken token)
        {
            var locations = new List<Location>
            {
                new Location
                {
                    Uri = "file:///source.txt",
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 10, Character = 5 },
                        End = new Position { Line = 10, Character = 20 }
                    }
                }
            };

            return Task.FromResult<DefinitionResponse?>(new DefinitionResponse(locations));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.DefinitionProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnDefinitionLocations()
    {
        // Arrange
        var handler = new TestDefinitionHandler();
        var request = new DefinitionParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Position = new Position { Line = 5, Character = 10 }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<DefinitionResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Locations.Should().HaveCount(1);
        response.Locations[0].Uri.Should().Be("file:///source.txt");
        response.Locations[0].Range.Start.Line.Should().Be(10);
    }

    [Fact]
    public void RegisterCapability_ShouldEnableDefinitionProvider()
    {
        // Arrange
        var handler = new TestDefinitionHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.DefinitionProvider.Should().NotBeNull();
    }
}
