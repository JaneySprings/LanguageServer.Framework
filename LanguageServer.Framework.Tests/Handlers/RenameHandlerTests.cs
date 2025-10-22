using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class RenameHandlerTests : TestHandlerBase
{
    private class TestRenameHandler : RenameHandlerBase
    {
        protected override Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken token)
        {
            var workspaceEdit = new WorkspaceEdit
            {
                Changes = new Dictionary<string, List<TextEdit>>
                {
                    ["file:///test.txt"] = new List<TextEdit>
                    {
                        new TextEdit
                        {
                            Range = new LocationRange
                            {
                                Start = new Position { Line = 5, Character = 10 },
                                End = new Position { Line = 5, Character = 20 }
                            },
                            NewText = request.NewName
                        }
                    },
                    ["file:///test2.txt"] = new List<TextEdit>
                    {
                        new TextEdit
                        {
                            Range = new LocationRange
                            {
                                Start = new Position { Line = 10, Character = 5 },
                                End = new Position { Line = 10, Character = 15 }
                            },
                            NewText = request.NewName
                        }
                    }
                }
            };

            return Task.FromResult<WorkspaceEdit?>(workspaceEdit);
        }

        protected override Task<PrepareRenameResponse> Handle(PrepareRenameParams request, CancellationToken token)
        {
            return Task.FromResult<PrepareRenameResponse?>(new PrepareRenameResponse
            {
                Range = new LocationRange
                {
                    Start = new Position { Line = 5, Character = 10 },
                    End = new Position { Line = 5, Character = 20 }
                },
                Placeholder = "oldName"
            });
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.RenameProvider = new Protocol.Capabilities.Server.Options.RenameOptions
            {
                PrepareProvider = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnWorkspaceEdit()
    {
        // Arrange
        var handler = new TestRenameHandler();
        var request = new RenameParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Position = new Position { Line = 5, Character = 15 },
            NewName = "newName"
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(RenameParams), typeof(CancellationToken) }, null)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<WorkspaceEdit?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Changes.Should().HaveCount(2);
        response.Changes!["file:///test.txt"][0].NewText.Should().Be("newName");
    }

    [Fact]
    public async Task PrepareRename_ShouldReturnRenameRange()
    {
        // Arrange
        var handler = new TestRenameHandler();
        var request = new PrepareRenameParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Position = new Position { Line = 5, Character = 15 }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(PrepareRenameParams), typeof(CancellationToken) }, null)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<PrepareRenameResponse>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Range.Should().NotBeNull();
        response.Placeholder.Should().Be("oldName");
    }

    [Fact]
    public void RegisterCapability_ShouldSetRenameProvider()
    {
        // Arrange
        var handler = new TestRenameHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.RenameProvider.Should().NotBeNull();
        var options = serverCapabilities.RenameProvider as Protocol.Capabilities.Server.Options.RenameOptions;
        options.Should().NotBeNull();
        options!.PrepareProvider.Should().BeTrue();
    }
}
