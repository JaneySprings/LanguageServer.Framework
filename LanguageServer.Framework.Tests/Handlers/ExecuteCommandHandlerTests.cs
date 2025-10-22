using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.ExecuteCommand;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class ExecuteCommandHandlerTests : TestHandlerBase
{
    private class TestExecuteCommandHandler : ExecuteCommandHandlerBase
    {
        protected override Task<ExecuteCommandResponse> Handle(ExecuteCommandParams request, CancellationToken token)
        {
            var result = request.Command switch
            {
                "test.command1" => "Command 1 executed",
                "test.command2" => "Command 2 executed",
                _ => "Unknown command"
            };

            return Task.FromResult(new ExecuteCommandResponse(result));
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities,
            ClientCapabilities clientCapabilities)
        {
            serverCapabilities.ExecuteCommandProvider = new Protocol.Capabilities.Server.Options.ExecuteCommandOptions
            {
                Commands =
                [
                    "test.command1",
                    "test.command2"
                ]
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldExecuteCommand()
    {
        // Arrange
        var handler = new TestExecuteCommandHandler();
        var request = new ExecuteCommandParams
        {
            Command = "test.command1",
            Arguments = []
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<ExecuteCommandResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().Be("Command 1 executed");
    }

    [Fact]
    public async Task Handle_WithUnknownCommand_ShouldReturnUnknownMessage()
    {
        // Arrange
        var handler = new TestExecuteCommandHandler();
        var request = new ExecuteCommandParams
        {
            Command = "unknown.command",
            Arguments = []
        };

        // Act
        var method = handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = method.Invoke(handler, [request, CancellationToken.None]);

        var response = await (task as Task<ExecuteCommandResponse?>)!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().Be("Unknown command");
    }

    [Fact]
    public void RegisterCapability_ShouldSetExecuteCommandProvider()
    {
        // Arrange
        var handler = new TestExecuteCommandHandler();
        var serverCapabilities = new ServerCapabilities();
        var clientCapabilities = new ClientCapabilities();

        // Act
        handler.RegisterCapability(serverCapabilities, clientCapabilities);

        // Assert
        serverCapabilities.ExecuteCommandProvider.Should().NotBeNull();
        serverCapabilities.ExecuteCommandProvider!.Commands.Should().Contain("test.command1");
        serverCapabilities.ExecuteCommandProvider.Commands.Should().Contain("test.command2");
    }
}