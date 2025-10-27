using FluentAssertions;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class GetCommandHandlerTests : TestBase
{
    [Theory]
    [InlineData("get", TestDisplayName = "LowercaseGet")]
    [InlineData("GET", TestDisplayName = "UppercaseGet")]
    [InlineData("Get", TestDisplayName = "MixedCaseGet")]
    [InlineData("gEt", TestDisplayName = "MixedCaseVariant")]
    public void CanHandle_Should_ReturnTrue_When_CommandNameIsGet(string commandName)
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var handler = new GetCommandHandler(repo.Object);
        var command = new Command(commandName, new WorkerId(1), new Dictionary<string, string>());

        // Act
        var result = handler.CanHandle(command);

        // Assert
        result.Should().BeTrue(because: $"handler should recognize '{commandName}' command case-insensitively");
    }

    [Theory]
    [InlineData("set", TestDisplayName = "SetCommand")]
    [InlineData("delete", TestDisplayName = "DeleteCommand")]
    [InlineData("create", TestDisplayName = "CreateCommand")]
    [InlineData("unknown", TestDisplayName = "UnknownCommand")]
    public void CanHandle_Should_ReturnFalse_When_CommandNameIsNotGet(string commandName)
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var handler = new GetCommandHandler(repo.Object);
        var command = new Command(commandName, new WorkerId(1), new Dictionary<string, string>());

        // Act
        var result = handler.CanHandle(command);

        // Assert
        result.Should().BeFalse(because: $"handler should not recognize '{commandName}' command");
    }

    [Fact]
    public async Task Handle_Should_ReturnErrorMessage_When_WorkerIdIsEmpty()
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var handler = new GetCommandHandler(repo.Object);
        var command = new Command("get", WorkerId.Empty, new Dictionary<string, string>());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be("WorkerId is required for get command", because: "handler should return validation error for empty WorkerId");
        repo.Verify(x => x.Get(It.IsAny<WorkerId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnErrorMessage_When_CommandHasParameters()
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var handler = new GetCommandHandler(repo.Object);
        var parameters = new Dictionary<string, string> { { "key", "value" } };
        var command = new Command("get", new WorkerId(1), parameters);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be("Get command can't have parameters", because: "handler should return validation error when parameters are provided");
        repo.Verify(x => x.Get(It.IsAny<WorkerId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnErrorMessage_When_ConfigurationNotFound()
    {
        // Arrange
        var workerId = new WorkerId(123);
        var repo = new Mock<IWorkerConfigurationRepo>();
        var handler = new GetCommandHandler(repo.Object);
        var command = new Command("get", workerId, new Dictionary<string, string>());
        var cancellationToken = CancellationToken.None;

        repo
            .Setup(x => x.Get(workerId, cancellationToken))
            .ReturnsAsync((WorkerConfiguration?)null);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be($"Worker with id {workerId} not found", because: "handler should return error message when configuration is not found");
        repo.Verify(x => x.Get(workerId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnConfigurationString_When_AllValidationsPass()
    {
        // Arrange
        var workerId = new WorkerId(456);
        var repo = new Mock<IWorkerConfigurationRepo>();
        var handler = new GetCommandHandler(repo.Object);
        var command = new Command("get", workerId, new Dictionary<string, string>());
        var cancellationToken = CancellationToken.None;

        var providerInfo = new ProviderInfo(ProviderType.Github, "TestTarget");
        var clientInfo = new ClientInfo(ClientType.Telegram, "ClientTarget");
        var configuration = new WorkerConfiguration(
            workerId,
            providerInfo,
            "* * * * *",
            "filter",
            clientInfo,
            "template"
        );

        repo
            .Setup(x => x.Get(workerId, cancellationToken))
            .ReturnsAsync(configuration);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(configuration.ToString(), because: "handler should return the string representation of the configuration");
        repo.Verify(x => x.Get(workerId, cancellationToken), Times.Once);
    }
}