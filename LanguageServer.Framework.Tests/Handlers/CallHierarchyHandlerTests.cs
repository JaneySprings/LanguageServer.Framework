using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class CallHierarchyHandlerTests : TestHandlerBase
{
    private class TestCallHierarchyHandler : CallHierarchyHandlerBase
    {
        protected override Task<CallHierarchyPrepareResponse?> CallHierarchyPrepare(CallHierarchyPrepareParams request,
            CancellationToken token)
        {
            var items = new List<CallHierarchyItem>
            {
                new CallHierarchyItem
                {
                    Name = "testFunction",
                    Kind = SymbolKind.Function,
                    Uri = "file:///test.txt",
                    Range = new DocumentRange(
                        new Position { Line = 5, Character = 0 },
                        new Position { Line = 10, Character = 0 }
                    ),
                    SelectionRange = new DocumentRange(
                        new Position { Line = 5, Character = 9 },
                        new Position { Line = 5, Character = 21 }
                    )
                }
            };

            return Task.FromResult<CallHierarchyPrepareResponse?>(new CallHierarchyPrepareResponse(items));
        }

        protected override Task<CallHierarchyIncomingCallsResponse> CallHierarchyIncomingCalls(
            CallHierarchyIncomingCallsParams request, CancellationToken token)
        {
            var calls = new List<CallHierarchyIncomingCall>
            {
                new CallHierarchyIncomingCall
                {
                    From = new Location
                    {
                        Uri = "file:///caller.txt",
                        Range = new DocumentRange(
                            new Position { Line = 15, Character = 0 },
                            new Position { Line = 20, Character = 0 }
                        )
                    },
                    FromRanges = new DocumentRange(
                        new Position { Line = 17, Character = 4 },
                        new Position { Line = 17, Character = 16 }
                    )
                }
            };

            return Task.FromResult(new CallHierarchyIncomingCallsResponse(calls));
        }

        protected override Task<CallHierarchyOutgoingCallsResponse> CallHierarchyOutgoingCalls(
            CallHierarchyOutgoingCallsParams request, CancellationToken token)
        {
            var calls = new List<CallHierarchyOutgoingCall>
            {
                new CallHierarchyOutgoingCall
                {
                    To = new Location
                    {
                        Uri = "file:///callee.txt",
                        Range = new DocumentRange(
                            new Position { Line = 25, Character = 0 },
                            new Position { Line = 30, Character = 0 }
                        )
                    },
                    FromRanges = new DocumentRange(
                        new Position { Line = 7, Character = 4 },
                        new Position { Line = 7, Character = 10 }
                    )
                }
            };

            return Task.FromResult(new CallHierarchyOutgoingCallsResponse(calls));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.CallHierarchyProvider = true;
        }
    }

    [Fact]
    public async Task Prepare_ShouldReturnCallHierarchyItems()
    {
        // Arrange
        var handler = new TestCallHierarchyHandler();
        var request = new CallHierarchyPrepareParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Position = new Position { Line = 5, Character = 15 }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("CallHierarchyPrepare",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<CallHierarchyPrepareResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().HaveCount(1);
        response.Result![0].Name.Should().Be("testFunction");
        response.Result[0].Kind.Should().Be(SymbolKind.Function);
    }

    [Fact]
    public async Task IncomingCalls_ShouldReturnIncomingCalls()
    {
        // Arrange
        var handler = new TestCallHierarchyHandler();
        var request = new CallHierarchyIncomingCallsParams
        {
            Item = new CallHierarchyItem
            {
                Name = "testFunction",
                Kind = SymbolKind.Function,
                Uri = "file:///test.txt",
                Range = new DocumentRange(new Position(), new Position()),
                SelectionRange = new DocumentRange(new Position(), new Position())
            }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("CallHierarchyIncomingCalls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<CallHierarchyIncomingCallsResponse>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().HaveCount(1);
        response.Result[0].From.Uri.Should().Be("file:///caller.txt");
        response.Result[0].From.Range.Start.Line.Should().Be(15);
    }

    [Fact]
    public async Task OutgoingCalls_ShouldReturnOutgoingCalls()
    {
        // Arrange
        var handler = new TestCallHierarchyHandler();
        var request = new CallHierarchyOutgoingCallsParams
        {
            Item = new CallHierarchyItem
            {
                Name = "testFunction",
                Kind = SymbolKind.Function,
                Uri = "file:///test.txt",
                Range = new DocumentRange(new Position(), new Position()),
                SelectionRange = new DocumentRange(new Position(), new Position())
            }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("CallHierarchyOutgoingCalls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<CallHierarchyOutgoingCallsResponse>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().HaveCount(1);
        response.Result[0].To.Uri.Should().Be("file:///callee.txt");
        response.Result[0].To.Range.Start.Line.Should().Be(25);
    }

    [Fact]
    public void RegisterCapability_ShouldEnableCallHierarchyProvider()
    {
        // Arrange
        var handler = new TestCallHierarchyHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.CallHierarchyProvider.Should().NotBeNull();
    }
}