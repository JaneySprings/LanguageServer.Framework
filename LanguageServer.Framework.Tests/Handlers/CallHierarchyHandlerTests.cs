using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
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
        protected override Task<CallHierarchyPrepareResponse?> CallHierarchyPrepare(CallHierarchyPrepareParams request, CancellationToken token)
        {
            var items = new List<CallHierarchyItem>
            {
                new CallHierarchyItem
                {
                    Name = "testFunction",
                    Kind = SymbolKind.Function,
                    Uri = "file:///test.txt",
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 5, Character = 0 },
                        End = new Position { Line = 10, Character = 0 }
                    },
                    SelectionRange = new LocationRange
                    {
                        Start = new Position { Line = 5, Character = 9 },
                        End = new Position { Line = 5, Character = 21 }
                    }
                }
            };

            return Task.FromResult<CallHierarchyPrepareResponse?>(new CallHierarchyPrepareResponse(items));
        }

        protected override Task<CallHierarchyIncomingCallsResponse> CallHierarchyIncomingCalls(CallHierarchyIncomingCallsParams request, CancellationToken token)
        {
            var calls = new List<CallHierarchyIncomingCall>
            {
                new CallHierarchyIncomingCall
                {
                    From = new CallHierarchyItem
                    {
                        Name = "caller",
                        Kind = SymbolKind.Function,
                        Uri = "file:///caller.txt",
                        Range = new LocationRange
                        {
                            Start = new Position { Line = 15, Character = 0 },
                            End = new Position { Line = 20, Character = 0 }
                        },
                        SelectionRange = new LocationRange
                        {
                            Start = new Position { Line = 15, Character = 9 },
                            End = new Position { Line = 15, Character = 15 }
                        }
                    },
                    FromRanges = new List<LocationRange>
                    {
                        new LocationRange
                        {
                            Start = new Position { Line = 17, Character = 4 },
                            End = new Position { Line = 17, Character = 16 }
                        }
                    }
                }
            };

            return Task.FromResult<CallHierarchyIncomingCallsResponse?>(new CallHierarchyIncomingCallsResponse(calls));
        }

        protected override Task<CallHierarchyOutgoingCallsResponse> CallHierarchyOutgoingCalls(CallHierarchyOutgoingCallsParams request, CancellationToken token)
        {
            var calls = new List<CallHierarchyOutgoingCall>
            {
                new CallHierarchyOutgoingCall
                {
                    To = new CallHierarchyItem
                    {
                        Name = "callee",
                        Kind = SymbolKind.Function,
                        Uri = "file:///callee.txt",
                        Range = new LocationRange
                        {
                            Start = new Position { Line = 25, Character = 0 },
                            End = new Position { Line = 30, Character = 0 }
                        },
                        SelectionRange = new LocationRange
                        {
                            Start = new Position { Line = 25, Character = 9 },
                            End = new Position { Line = 25, Character = 15 }
                        }
                    },
                    FromRanges = new List<LocationRange>
                    {
                        new LocationRange
                        {
                            Start = new Position { Line = 7, Character = 4 },
                            End = new Position { Line = 7, Character = 10 }
                        }
                    }
                }
            };

            return Task.FromResult<CallHierarchyOutgoingCallsResponse?>(new CallHierarchyOutgoingCallsResponse(calls));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
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
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Position = new Position { Line = 5, Character = 15 }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("CallHierarchyPrepare", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<CallHierarchyPrepareResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.CallHierarchyItems.Should().HaveCount(1);
        response.CallHierarchyItems[0].Name.Should().Be("testFunction");
        response.CallHierarchyItems[0].Kind.Should().Be(SymbolKind.Function);
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
                Range = new LocationRange { Start = new Position(), End = new Position() },
                SelectionRange = new LocationRange { Start = new Position(), End = new Position() }
            }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("CallHierarchyIncomingCalls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<CallHierarchyIncomingCallsResponse>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.IncomingCalls.Should().HaveCount(1);
        response.IncomingCalls[0].From.Name.Should().Be("caller");
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
                Range = new LocationRange { Start = new Position(), End = new Position() },
                SelectionRange = new LocationRange { Start = new Position(), End = new Position() }
            }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("CallHierarchyOutgoingCalls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<CallHierarchyOutgoingCallsResponse>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.OutgoingCalls.Should().HaveCount(1);
        response.OutgoingCalls[0].To.Name.Should().Be("callee");
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
