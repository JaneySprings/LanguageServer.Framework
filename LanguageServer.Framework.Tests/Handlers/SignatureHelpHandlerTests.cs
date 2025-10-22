using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SignatureHelp;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Markup;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class SignatureHelpHandlerTests : TestHandlerBase
{
    private class TestSignatureHelpHandler : SignatureHelpHandlerBase
    {
        protected override Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken token)
        {
            return Task.FromResult<SignatureHelpResponse?>(new SignatureHelpResponse
            {
                Signatures =
                [
                    new SignatureInformation
                    {
                        Label = "testFunction(param1: string, param2: number)",
                        Documentation = new MarkupContent
                        {
                            Kind = MarkupKind.Markdown,
                            Value = "Test function documentation"
                        },
                        Parameters =
                        [
                            new ParameterInformation
                            {
                                Label = "param1",
                                Documentation = new MarkupContent
                                {
                                    Kind = MarkupKind.PlainText,
                                    Value = "First parameter"
                                }
                            },
                            new ParameterInformation
                            {
                                Label = "param2",
                                Documentation = new MarkupContent
                                {
                                    Kind = MarkupKind.PlainText,
                                    Value = "Second parameter"
                                }
                            }
                        ]
                    }
                ],
                ActiveSignature = 0,
                ActiveParameter = 0
            })!;
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.SignatureHelpProvider = new Protocol.Capabilities.Server.Options.SignatureHelpOptions
            {
                TriggerCharacters = ["(", ","]
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnSignatureHelp()
    {
        // Arrange
        var handler = new TestSignatureHelpHandler();
        var request = new SignatureHelpParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Position = new Position { Line = 5, Character = 15 }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<SignatureHelp?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Signatures.Should().HaveCount(1);
        response.Signatures[0].Label.Should().Contain("testFunction");
        response.Signatures[0].Parameters.Should().HaveCount(2);
        response.ActiveSignature.Should().Be(0);
        response.ActiveParameter.Should().Be(0);
    }

    [Fact]
    public void RegisterCapability_ShouldSetSignatureHelpProvider()
    {
        // Arrange
        var handler = new TestSignatureHelpHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.SignatureHelpProvider.Should().NotBeNull();
        serverCapabilities.SignatureHelpProvider!.TriggerCharacters.Should().Contain("(");
        serverCapabilities.SignatureHelpProvider.TriggerCharacters.Should().Contain(",");
    }
}
