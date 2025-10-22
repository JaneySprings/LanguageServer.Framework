using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers.Working;

/// <summary>
/// 可工作的 CompletionHandler 测试示例
/// </summary>
public class WorkingCompletionHandlerTests
{
    private class TestCompletionHandler : CompletionHandlerBase
    {
        protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
        {
            var items = new List<CompletionItem>
            {
                new CompletionItem
                {
                    Label = "testMethod",
                    Kind = CompletionItemKind.Method,
                    Detail = "void testMethod()",
                    Documentation = "Test method documentation",
                    InsertText = "testMethod()",
                    InsertTextFormat = InsertTextFormat.PlainText
                },
                new CompletionItem
                {
                    Label = "testVariable",
                    Kind = CompletionItemKind.Variable,
                    Detail = "string testVariable",
                    InsertText = "testVariable"
                }
            };

            return Task.FromResult<CompletionResponse?>(new CompletionResponse(items));
        }

        protected override Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
        {
            // 为 resolve 添加额外信息
            item.Documentation = item.Documentation ?? "Resolved documentation";
            return Task.FromResult(item);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.CompletionProvider = new Protocol.Capabilities.Server.Options.CompletionOptions
            {
                TriggerCharacters = [".", ":"],
                ResolveProvider = true
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnCompletionItems()
    {
        // Arrange
        var handler = new TestCompletionHandler();

        var request = new CompletionParams
        {
            TextDocument = new TextDocumentIdentifier("file:///test.txt"),
            Position = new Position { Line = 0, Character = 5 }
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = method.Invoke(handler, [request, CancellationToken.None]);
        var response = await (task as Task<CompletionResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Items.Should().NotBeNull();
        response.Items.Should().HaveCount(2);
        response.Items![0].Label.Should().Be("testMethod");
        response.Items[0].Kind.Should().Be(CompletionItemKind.Method);
        response.Items[1].Label.Should().Be("testVariable");
        response.Items[1].Kind.Should().Be(CompletionItemKind.Variable);
    }

    [Fact]
    public async Task Resolve_ShouldAddDocumentation()
    {
        // Arrange
        var handler = new TestCompletionHandler();
        var item = new CompletionItem
        {
            Label = "test",
            Kind = CompletionItemKind.Function
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = method.Invoke(handler, [item, CancellationToken.None]);
        var resolved = await (task as Task<CompletionItem>)!;

        // Assert
        resolved.Should().NotBeNull();
        resolved.Documentation.Should().Be("Resolved documentation");
    }

    [Fact]
    public void RegisterCapability_ShouldSetCompletionProvider()
    {
        // Arrange
        var handler = new TestCompletionHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.CompletionProvider.Should().NotBeNull();
        serverCapabilities.CompletionProvider!.ResolveProvider.Should().BeTrue();
        serverCapabilities.CompletionProvider.TriggerCharacters.Should().Contain(".");
        serverCapabilities.CompletionProvider.TriggerCharacters.Should().Contain(":");
    }
}
