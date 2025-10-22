using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeLens;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class CodeLensHandlerTests : TestHandlerBase
{
    private class TestCodeLensHandler : CodeLensHandlerBase
    {
        protected override Task<CodeLensResponse> Handle(CodeLensParams request, CancellationToken token)
        {
            var lenses = new List<CodeLens>
            {
                new()
                {
                    Range = new DocumentRange(
                        new Position { Line = 0, Character = 0 },
                        new Position { Line = 0, Character = 10 }
                    ),
                    Command = new Command
                    {
                        Title = "5 references",
                        Name = "editor.action.showReferences"
                    }
                }
            };

            return Task.FromResult(new CodeLensResponse(lenses));
        }

        protected override Task<CodeLens> Resolve(CodeLens codeLens, CancellationToken token)
        {
            codeLens.Command = new Command
            {
                Title = "10 references (resolved)",
                Name = "editor.action.showReferences"
            };
            return Task.FromResult(codeLens);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.CodeLensProvider = new Protocol.Capabilities.Server.Options.CodeLensOptions
            {
                ResolveProvider = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnCodeLenses()
    {
        // Arrange
        var handler = new TestCodeLensHandler();
        var request = new CodeLensParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt")
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<CodeLensResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.CodeLenses.Should().HaveCount(1);
        response.CodeLenses[0].Command.Should().NotBeNull();
        response.CodeLenses[0].Command!.Title.Should().Contain("references");
    }

    [Fact]
    public async Task Resolve_ShouldUpdateCodeLens()
    {
        // Arrange
        var handler = new TestCodeLensHandler();
        var lens = new CodeLens
        {
            Range = new DocumentRange(
                new Position { Line = 5, Character = 0 },
                new Position { Line = 5, Character = 10 }
            )
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [lens, CancellationToken.None]);

        var resolved = await (task as Task<CodeLens>)!;

        // Assert
        resolved.Should().NotBeNull();
        resolved.Command.Should().NotBeNull();
        resolved.Command!.Title.Should().Contain("resolved");
    }

    [Fact]
    public void RegisterCapability_ShouldSetCodeLensProvider()
    {
        // Arrange
        var handler = new TestCodeLensHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.CodeLensProvider.Should().NotBeNull();
        serverCapabilities.CodeLensProvider!.ResolveProvider.Should().BeTrue();
    }
}