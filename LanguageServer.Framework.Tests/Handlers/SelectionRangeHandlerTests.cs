using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SelectionRange;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class SelectionRangeHandlerTests : TestHandlerBase
{
    private class TestSelectionRangeHandler : SelectionRangeHandlerBase
    {
        protected override Task<SelectionRangeResponse?> Handle(SelectionRangeParams request, CancellationToken token)
        {
            var ranges = new List<SelectionRange>
            {
                new SelectionRange
                {
                    Range = new DocumentRange(
                        new Position { Line = 5, Character = 10 },
                        new Position { Line = 5, Character = 20 }
                    ),
                    Parent = new SelectionRange
                    {
                        Range = new DocumentRange(
                            new Position { Line = 5, Character = 5 },
                            new Position { Line = 5, Character = 25 }
                        )
                    }
                }
            };

            return Task.FromResult<SelectionRangeResponse?>(new SelectionRangeResponse(ranges));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.SelectionRangeProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnSelectionRanges()
    {
        // Arrange
        var handler = new TestSelectionRangeHandler();
        var request = new SelectionRangeParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Positions = [new Position { Line = 5, Character = 15 }]
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<SelectionRangeResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Ranges.Should().HaveCount(1);
        response.Ranges[0].Range.Should().NotBeNull();
        response.Ranges[0].Parent.Should().NotBeNull();
    }

    [Fact]
    public void RegisterCapability_ShouldEnableSelectionRangeProvider()
    {
        // Arrange
        var handler = new TestSelectionRangeHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.SelectionRangeProvider.Should().NotBeNull();
    }
}