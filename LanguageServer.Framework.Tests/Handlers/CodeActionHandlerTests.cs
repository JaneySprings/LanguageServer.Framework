using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
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
        protected override Task<CodeActionResponse> Handle(CodeActionParams request, CancellationToken token)
        {
            var actions = new List<CodeAction>
            {
                new()
                {
                    Title = "Fix spelling",
                    Kind = CodeActionKind.QuickFix,
                    Edit = new WorkspaceEdit
                    {
                        Changes = new Dictionary<DocumentUri, List<TextEdit>>
                        {
                            [new DocumentUri(new Uri("file:///test.txt"))] =
                            [
                                new TextEdit
                                {
                                    Range = new DocumentRange(
                                        new Position { Line = 0, Character = 0 },
                                        new Position { Line = 0, Character = 5 }
                                    ),
                                    NewText = "Fixed"
                                }
                            ]
                        }
                    }
                },
                new()
                {
                    Title = "Refactor method",
                    Kind = CodeActionKind.Refactor
                }
            };

            return Task.FromResult(new CodeActionResponse(actions));
        }

        protected override Task<CodeAction?> Resolve(CodeAction codeAction, CancellationToken token)
        {
            // Add command to resolved action
            codeAction.Command = new Command
            {
                Title = codeAction.Title,
                Name = "refactor.execute"
            };
            return Task.FromResult(codeAction)!;
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
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
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Range = new DocumentRange(
                new Position { Line = 5, Character = 0 },
                new Position { Line = 5, Character = 10 }
            ),
            Context = new CodeActionContext
            {
                Diagnostics = []
            }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<CodeActionResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.CommandOrCodeActions.Should().HaveCount(2);

        response.CommandOrCodeActions[0].Command?.Title.Should().Be("Fix spelling");
        response.CommandOrCodeActions[0].CodeAction?.Kind.Should().Be(CodeActionKind.QuickFix);
        response.CommandOrCodeActions[1].Command?.Title.Should().Be("Refactor method");
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
        var method = handler.GetType()
            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [action, CancellationToken.None]);

        var resolved = await (task as Task<CodeAction?>)!;

        // Assert
        resolved.Should().NotBeNull();
        resolved!.Command.Should().NotBeNull();
        resolved.Command!.Name.Should().Be("refactor.execute");
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
        var options = serverCapabilities.CodeActionProvider;
        options.Should().NotBeNull();
        options!.Value!.ResolveProvider.Should().BeTrue();
        options.Value.CodeActionKinds.Should().Contain(CodeActionKind.QuickFix);
    }
}