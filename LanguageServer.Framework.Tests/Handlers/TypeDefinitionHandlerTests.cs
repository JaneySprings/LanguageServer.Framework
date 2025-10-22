using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeDefinition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class TypeDefinitionHandlerTests : TestHandlerBase
{
    private class TestTypeDefinitionHandler : TypeDefinitionHandlerBase
    {
        protected override Task<TypeDefinitionResponse?> Handle(TypeDefinitionParams request, CancellationToken token)
        {
            var locations = new List<Location>
            {
                new Location
                {
                    Uri = "file:///types.txt",
                    Range = new DocumentRange(
                        new Position { Line = 0, Character = 0 },
                        new Position { Line = 10, Character = 0 }
                    )
                }
            };

            return Task.FromResult<TypeDefinitionResponse?>(new TypeDefinitionResponse(locations));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.TypeDefinitionProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnTypeDefinitionLocations()
    {
        // Arrange
        var handler = new TestTypeDefinitionHandler();
        var request = new TypeDefinitionParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Position = new Position { Line = 5, Character = 10 }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<TypeDefinitionResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result2.Should().HaveCount(1);
        response.Result2![0].Uri.Should().Be("file:///types.txt");
    }

    [Fact]
    public void RegisterCapability_ShouldEnableTypeDefinitionProvider()
    {
        // Arrange
        var handler = new TestTypeDefinitionHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.TypeDefinitionProvider.Should().NotBeNull();
    }
}