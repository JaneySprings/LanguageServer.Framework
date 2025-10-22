using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.ExecuteCommand;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;
using LanguageServerType = EmmyLua.LanguageServer.Framework.Server.LanguageServer;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class ExecuteCommandHandlerTests : TestHandlerBase
{
    private class TestExecuteCommandHandler : ExecuteCommandHandlerBase
    {
        public TestExecuteCommandHandler(LanguageServerType server) : base(server)
        {
        }

        protected override Task<ExecuteCommandResponse?> Handle(ExecuteCommandParams request, CancellationToken token)
        {
            var result = request.Command switch
            {
                "test.command1" => "Command 1 executed",
                "test.command2" => "Command 2 executed",
                _ => "Unknown command"
            };

            return Task.FromResult<ExecuteCommandResponse?>(new ExecuteCommandResponse
            {
                Result = result
            });
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            serverCapabilities.ExecuteCommandProvider = new Protocol.Capabilities.Server.Options.ExecuteCommandOptions
            {
                Commands = new List<string>
                {
                    "test.command1",
                    "test.command2"
                }
            };
        }
    }

    [Fact]
    public async Task Handle_ShouldExecuteCommand()
    {
        // Arrange
        var handler = new TestExecuteCommandHandler(Server);
        var request = new ExecuteCommandParams
        {
            Command = "test.command1",
            Arguments = []
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<ExecuteCommandResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().Be("Command 1 executed");
    }

    [Fact]
    public async Task Handle_WithUnknownCommand_ShouldReturnUnknownMessage()
    {
        // Arrange
        var handler = new TestExecuteCommandHandler(Server);
        var request = new ExecuteCommandParams
        {
            Command = "unknown.command",
            Arguments = []
        };

        // Act
        var result = await handler.GetType()
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(handler, new object[] { request, CancellationToken.None }) as Task<ExecuteCommandResponse?>;

        var response = await result!;

        // Assert
        response.Should().NotBeNull();
        response!.Result.Should().Be("Unknown command");
    }

    [Fact]
    public void RegisterCapability_ShouldSetExecuteCommandProvider()
    {
        // Arrange
        var handler = new TestExecuteCommandHandler(Server);
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
