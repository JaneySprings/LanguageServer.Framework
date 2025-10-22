using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Implementation;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class ImplementationHandlerTests : TestHandlerBase
{
    private class TestImplementationHandler : ImplementationHandlerBase
    {
        protected override Task<ImplementationResponse?> Handle(ImplementationParams request, CancellationToken token)
        {
            var locations = new List<Location>
            {
                new Location
                {
                    Uri = "file:///implementation1.txt",
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 10, Character = 0 },
                        End = new Position { Line = 20, Character = 0 }
                    }
                },
                new Location
                {
                    Uri = "file:///implementation2.txt",
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 5, Character = 0 },
                        End = new Position { Line = 15, Character = 0 }
                    }
                }
            };

            return Task.FromResult<ImplementationResponse?>(new ImplementationResponse(locations));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.ImplementationProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnImplementationLocations()
    {
        // Arrange
        var handler = new TestImplementationHandler();
        var request = new ImplementationParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///interface.txt" },
            Position = new Position { Line = 5, Character = 10 }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<ImplementationResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Locations.Should().HaveCount(2);
        response.Locations[0].Uri.Should().Be("file:///implementation1.txt");
        response.Locations[1].Uri.Should().Be("file:///implementation2.txt");
    }

    [Fact]
    public void RegisterCapability_ShouldEnableImplementationProvider()
    {
        // Arrange
        var handler = new TestImplementationHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.ImplementationProvider.Should().NotBeNull();
    }
}
