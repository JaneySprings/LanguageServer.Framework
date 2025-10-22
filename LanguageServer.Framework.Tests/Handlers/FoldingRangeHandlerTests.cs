using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class FoldingRangeHandlerTests : TestHandlerBase
{
    private class TestFoldingRangeHandler : FoldingRangeHandlerBase
    {
        protected override Task<FoldingRangeResponse?> Handle(FoldingRangeParams request, CancellationToken token)
        {
            var ranges = new List<FoldingRange>
            {
                new FoldingRange
                {
                    StartLine = 0,
                    EndLine = 10,
                    Kind = FoldingRangeKind.Region
                },
                new FoldingRange
                {
                    StartLine = 5,
                    EndLine = 8,
                    StartCharacter = 4,
                    EndCharacter = 4,
                    Kind = FoldingRangeKind.Comment
                }
            };

            return Task.FromResult<FoldingRangeResponse?>(new FoldingRangeResponse(ranges));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.FoldingRangeProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnFoldingRanges()
    {
        // Arrange
        var handler = new TestFoldingRangeHandler();
        var request = new FoldingRangeParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<FoldingRangeResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.FoldingRanges.Should().HaveCount(2);
        response.FoldingRanges[0].StartLine.Should().Be(0);
        response.FoldingRanges[0].EndLine.Should().Be(10);
        response.FoldingRanges[0].Kind.Should().Be(FoldingRangeKind.Region);
        response.FoldingRanges[1].Kind.Should().Be(FoldingRangeKind.Comment);
    }

    [Fact]
    public void RegisterCapability_ShouldEnableFoldingRangeProvider()
    {
        // Arrange
        var handler = new TestFoldingRangeHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.FoldingRangeProvider.Should().NotBeNull();
    }
}
