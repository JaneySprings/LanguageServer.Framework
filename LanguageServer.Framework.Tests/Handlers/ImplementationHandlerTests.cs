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
                    Range = new DocumentRange(
                        new Position { Line = 10, Character = 0 },
                        new Position { Line = 20, Character = 0 }
                    )
                },
                new Location
                {
                    Uri = "file:///implementation2.txt",
                    Range = new DocumentRange(
                        new Position { Line = 5, Character = 0 },
                        new Position { Line = 15, Character = 0 }
                    )
                }
            };

            return Task.FromResult<ImplementationResponse?>(new ImplementationResponse(locations));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
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
            TextDocument = new TextDocumentIdentifier("file:///interface.txt"),
            Position = new Position { Line = 5, Character = 10 }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<ImplementationResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result2.Should().HaveCount(2);
        response.Result2![0].Uri.Should().Be("file:///implementation1.txt");
        response.Result2[1].Uri.Should().Be("file:///implementation2.txt");
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