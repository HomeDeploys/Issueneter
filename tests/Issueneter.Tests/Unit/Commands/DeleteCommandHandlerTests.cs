using FluentAssertions;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class DeleteCommandHandlerTests : TestBase
{
    [Theory]
    [InlineData("delete", TestDisplayName = "LowercaseDelete")]
    [InlineData("DELETE", TestDisplayName = "UppercaseDelete")]
    [InlineData("Delete", TestDisplayName = "MixedCaseDelete")]
    [InlineData("dElEtE", TestDisplayName = "MixedCaseVariant")]
    public void CanHandle_Should_ReturnTrue_When_CommandNameIsDelete(string commandName)
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var scheduler = new Mock<IScheduler>();
        var handler = new DeleteCommandHandler(repo.Object, scheduler.Object);
        var command = new Command(commandName, new WorkerId(1), new Dictionary<string, string>());

        // Act
        var result = handler.CanHandle(command);

        // Assert
        result.Should().BeTrue(because: $"handler should recognize '{commandName}' command case-insensitively");
    }

    [Theory]
    [InlineData("set", TestDisplayName = "SetCommand")]
    [InlineData("get", TestDisplayName = "GetCommand")]
    [InlineData("create", TestDisplayName = "CreateCommand")]
    [InlineData("unknown", TestDisplayName = "UnknownCommand")]
    public void CanHandle_Should_ReturnFalse_When_CommandNameIsNotDelete(string commandName)
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var scheduler = new Mock<IScheduler>();
        var handler = new DeleteCommandHandler(repo.Object, scheduler.Object);
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
        var scheduler = new Mock<IScheduler>();
        var handler = new DeleteCommandHandler(repo.Object, scheduler.Object);
        var command = new Command("delete", WorkerId.Empty, new Dictionary<string, string>());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be("WorkerId is required for delete command", because: "handler should return validation error for empty WorkerId");
        repo.Verify(x => x.Delete(It.IsAny<WorkerId>(), It.IsAny<CancellationToken>()), Times.Never);
        scheduler.Verify(x => x.Deschedule(It.IsAny<WorkerId>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnErrorMessage_When_CommandHasParameters()
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var scheduler = new Mock<IScheduler>();
        var handler = new DeleteCommandHandler(repo.Object, scheduler.Object);
        var parameters = new Dictionary<string, string> { { "key", "value" } };
        var command = new Command("delete", new WorkerId(1), parameters);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be("Delete command can't have parameters", because: "handler should return validation error when parameters are provided");
        repo.Verify(x => x.Delete(It.IsAny<WorkerId>(), It.IsAny<CancellationToken>()), Times.Never);
        scheduler.Verify(x => x.Deschedule(It.IsAny<WorkerId>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_DeleteWorkerAndDescheduleIt_When_AllValidationsPass()
    {
        // Arrange
        var workerId = new WorkerId(123);
        var repo = new Mock<IWorkerConfigurationRepo>();
        var scheduler = new Mock<IScheduler>();
        var handler = new DeleteCommandHandler(repo.Object, scheduler.Object);
        var command = new Command("delete", workerId, new Dictionary<string, string>());
        var cancellationToken = CancellationToken.None;

        repo
            .Setup(x => x.Delete(workerId, cancellationToken))
            .Returns(Task.CompletedTask);
        scheduler
            .Setup(x => x.Deschedule(workerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be($"WorkerId {workerId} deleted", because: "handler should confirm the worker was deleted");
        repo.Verify(x => x.Delete(workerId, cancellationToken), Times.Once);
        scheduler.Verify(x => x.Deschedule(workerId), Times.Once);
    }

    [Fact]
    public void Metadata_Should_BeNonEmptyAndMatchDeleteCommandContract()
    {
        // Arrange
        var repo = new Mock<IWorkerConfigurationRepo>();
        var scheduler = new Mock<IScheduler>();
        var handler = new DeleteCommandHandler(repo.Object, scheduler.Object);

        // Assert
        handler.Name.Should().Be("delete", because: "delete handler should be identified by the 'delete' command name");
        handler.Description.Should().NotBeNullOrWhiteSpace(because: "handler must describe what the command does");
        handler.Example.Should().NotBeNullOrWhiteSpace(because: "handler must provide a usage example");
        handler.Arguments.Should().BeEmpty(because: "delete command only accepts a WorkerId in the first line, no Key: Value parameters");
    }
}
