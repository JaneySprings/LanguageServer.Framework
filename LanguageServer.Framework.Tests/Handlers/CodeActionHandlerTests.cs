using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class CodeActionHandlerTests : TestHandlerBase
{
    private class TestCodeActionHandler : CodeActionHandlerBase
    {
        protected override Task<CodeActionResponse?> Handle(CodeActionParams request, CancellationToken token)
        {
            var actions = new List<CodeAction>
            {
                new CodeAction
                {
                    Title = "Fix spelling",
                    Kind = CodeActionKind.QuickFix,
                    Edit = new WorkspaceEdit
                    {
                        Changes = new Dictionary<string, List<TextEdit>>
                        {
                            ["file:///test.txt"] = new List<TextEdit>
                            {
                                new TextEdit
                                {
                                    Range = new LocationRange
                                    {
                                        Start = new Position { Line = 0, Character = 0 },
                                        End = new Position { Line = 0, Character = 5 }
                                    },
                                    NewText = "Fixed"
                                }
                            }
                        }
                    }
                },
                new CodeAction
                {
                    Title = "Refactor method",
                    Kind = CodeActionKind.Refactor
                }
            };

            return Task.FromResult<CodeActionResponse?>(new CodeActionResponse(actions));
        }

        protected override Task<CodeAction?> Resolve(CodeAction codeAction, CancellationToken token)
        {
            // Add command to resolved action
            codeAction.Command = new Command
            {
                Title = codeAction.Title,
                CommandIdentifier = "refactor.execute"
            };
            return Task.FromResult<CodeAction?>(codeAction);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.CodeActionProvider = new Protocol.Capabilities.Server.Options.CodeActionOptions
            {
                CodeActionKinds =
                [
                    CodeActionKind.QuickFix,
                    CodeActionKind.Refactor
                ],
                ResolveProvider = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnCodeActions()
    {
        // Arrange
        var handler = new TestCodeActionHandler();
        var request = new CodeActionParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Range = new LocationRange
            {
                Start = new Position { Line = 5, Character = 0 },
                End = new Position { Line = 5, Character = 10 }
            },
            Context = new CodeActionContext
            {
                Diagnostics = []
            }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<CodeActionResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.CodeActions.Should().HaveCount(2);
        response.CodeActions[0].Title.Should().Be("Fix spelling");
        response.CodeActions[0].Kind.Should().Be(CodeActionKind.QuickFix);
        response.CodeActions[1].Title.Should().Be("Refactor method");
    }

    [Fact]
    public async Task Resolve_ShouldAddCommand()
    {
        // Arrange
        var handler = new TestCodeActionHandler();
        var action = new CodeAction
        {
            Title = "Test action",
            Kind = CodeActionKind.Refactor
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { action, CancellationToken.None }) as Task<CodeAction?>;

        var resolved = await result!;

        // Assert
        resolved.Should().NotBeNull();
        resolved!.Command.Should().NotBeNull();
        resolved.Command!.CommandIdentifier.Should().Be("refactor.execute");
    }

    [Fact]
    public void RegisterCapability_ShouldSetCodeActionProvider()
    {
        // Arrange
        var handler = new TestCodeActionHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.CodeActionProvider.Should().NotBeNull();
        var options = serverCapabilities.CodeActionProvider as Protocol.Capabilities.Server.Options.CodeActionOptions;
        options.Should().NotBeNull();
        options!.ResolveProvider.Should().BeTrue();
        options.CodeActionKinds.Should().Contain(CodeActionKind.QuickFix);
    }
}
