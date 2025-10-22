using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DocumentSymbolHandlerTests : TestHandlerBase
{
    private class TestDocumentSymbolHandler : DocumentSymbolHandlerBase
    {
        protected override Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken token)
        {
            var symbols = new List<DocumentSymbol>
            {
                new DocumentSymbol
                {
                    Name = "TestClass",
                    Kind = SymbolKind.Class,
                    Range = new DocumentRange(
                        new Position { Line = 0, Character = 0 },
                        new Position { Line = 20, Character = 0 }
                    ),
                    SelectionRange = new DocumentRange(
                        new Position { Line = 0, Character = 6 },
                        new Position { Line = 0, Character = 15 }
                    ),
                    Children =
                    [
                        new DocumentSymbol
                        {
                            Name = "TestMethod",
                            Kind = SymbolKind.Method,
                            Range = new DocumentRange(
                                new Position { Line = 5, Character = 4 },
                                new Position { Line = 10, Character = 4 }
                            ),
                            SelectionRange = new DocumentRange(
                                new Position { Line = 5, Character = 9 },
                                new Position { Line = 5, Character = 19 }
                            )
                        }
                    ]
                }
            };

            return Task.FromResult(new DocumentSymbolResponse(symbols));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.DocumentSymbolProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnDocumentSymbols()
    {
        // Arrange
        var handler = new TestDocumentSymbolHandler();
        var request = new DocumentSymbolParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt")
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<DocumentSymbolResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result1.Should().HaveCount(1);
        response.Result1[0].Name.Should().Be("TestClass");
        response.Result1[0].Kind.Should().Be(SymbolKind.Class);
        response.Result1[0].Children.Should().HaveCount(1);
        response.Result1[0].Children![0].Name.Should().Be("TestMethod");
    }

    [Fact]
    public void RegisterCapability_ShouldEnableDocumentSymbolProvider()
    {
        // Arrange
        var handler = new TestDocumentSymbolHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.DocumentSymbolProvider.Should().NotBeNull();
    }
}