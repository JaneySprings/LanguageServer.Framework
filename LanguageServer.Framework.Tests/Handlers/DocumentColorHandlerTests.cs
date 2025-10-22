using System.Drawing;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentColor;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class DocumentColorHandlerTests : TestHandlerBase
{
    private class TestDocumentColorHandler : DocumentColorHandlerBase
    {
        protected override Task<DocumentColorResponse?> Handle(DocumentColorParams request, CancellationToken token)
        {
            var colorInfos = new List<ColorInformation>
            {
                new ColorInformation
                {
                    Range = new LocationRange
                    {
                        Start = new Position { Line = 5, Character = 10 },
                        End = new Position { Line = 5, Character = 17 }
                    },
                    Color = new Color
                    {
                        Red = 1.0,
                        Green = 0.0,
                        Blue = 0.0,
                        Alpha = 1.0
                    }
                }
            };

            return Task.FromResult<DocumentColorResponse?>(new DocumentColorResponse(colorInfos))!;
        }

        protected override Task<ColorPresentationResponse> Resolve(ColorPresentationParams request, CancellationToken token)
        {
            var presentations = new List<ColorPresentation>
            {
                new ColorPresentation
                {
                    Label = $"rgb({request.Color.Red * 255}, {request.Color.Green * 255}, {request.Color.Blue * 255})"
                },
                new ColorPresentation
                {
                    Label = "#FF0000"
                }
            };

            return Task.FromResult<ColorPresentationResponse?>(new ColorPresentationResponse(presentations))!;
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.ColorProvider = true;
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnColorInformation()
    {
        // Arrange
        var handler = new TestDocumentColorHandler();
        var request = new DocumentColorParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<DocumentColorResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.ColorInfos.Should().HaveCount(1);
        response.ColorInfos[0].Color.Red.Should().Be(1.0);
        response.ColorInfos[0].Color.Green.Should().Be(0.0);
    }

    [Fact]
    public async Task HandlePresentation_ShouldReturnColorPresentations()
    {
        // Arrange
        var handler = new TestDocumentColorHandler();
        var request = new ColorPresentationParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
            Color = new Color
            {
                Red = 1.0,
                Green = 0.0,
                Blue = 0.0,
                Alpha = 1.0
            },
            Range = new LocationRange
            {
                Start = new Position { Line = 5, Character = 10 },
                End = new Position { Line = 5, Character = 17 }
            }
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Resolve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<ColorPresentationResponse>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.ColorPresentations.Should().HaveCount(2);
        response.ColorPresentations[1].Label.Should().Be("#FF0000");
    }

    [Fact]
    public void RegisterCapability_ShouldEnableColorProvider()
    {
        // Arrange
        var handler = new TestDocumentColorHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.ColorProvider.Should().NotBeNull();
    }
}
