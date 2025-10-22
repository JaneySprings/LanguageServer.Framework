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
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 5, Character = 10 },
                        End = new Position { Line = 5, Character = 20 }
                    },
                    Parent = new SelectionRange
                    {
                        Range = new LocationRange
                        {
                            Start = new Position { Line = 5, Character = 5 },
                            End = new Position { Line = 5, Character = 25 }
                        }
                    }
                }
            };

            return Task.FromResult<SelectionRangeResponse?>(new SelectionRangeResponse(ranges));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
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
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Positions = new List<Position>
            {
                new Position { Line = 5, Character = 15 }
            }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<SelectionRangeResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.SelectionRanges.Should().HaveCount(1);
        response.SelectionRanges[0].Range.Should().NotBeNull();
        response.SelectionRanges[0].Parent.Should().NotBeNull();
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
