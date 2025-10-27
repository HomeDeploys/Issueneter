using FluentAssertions;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class UpdateCommandHandlerTests : TestBase
{
    private readonly Mock<IWorkerConfigurationValidator> _mockValidator;
    private readonly Mock<IWorkerConfigurationRepo> _mockRepo;
    private readonly Mock<IScheduler> _mockScheduler;
    private readonly Mock<ITransactionProvider> _mockTransactionProvider;
    private readonly Mock<ITransaction> _mockTransaction;
    private readonly UpdateCommandHandler _handler;

    public UpdateCommandHandlerTests()
    {
        _mockValidator = new Mock<IWorkerConfigurationValidator>();
        _mockRepo = new Mock<IWorkerConfigurationRepo>();
        _mockScheduler = new Mock<IScheduler>();
        _mockTransactionProvider = new Mock<ITransactionProvider>();
        _mockTransaction = new Mock<ITransaction>();

        _handler = new UpdateCommandHandler(
            _mockValidator.Object,
            _mockRepo.Object,
            _mockScheduler.Object,
            _mockTransactionProvider.Object);
    }

    [Theory]
    [InlineData("update", TestDisplayName = "Lowercase")]
    [InlineData("UPDATE", TestDisplayName = "Uppercase")]
    [InlineData("Update", TestDisplayName = "Mixed case")]
    [InlineData("uPdAtE", TestDisplayName = "Random case")]
    public void CanHandle_Should_ReturnTrue_When_UpdateCommandProvided(string commandName)
    {
        // Arrange
        var command = CreateCommand(commandName);

        // Act
        var result = _handler.CanHandle(command);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_Should_ReturnFalse_When_RandomCommandProvided()
    {
        // Arrange
        var command = CreateCommand(Faker.Lorem.Word());

        // Act
        var result = _handler.CanHandle(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_ReturnWorkerIdRequiredError_When_CommandHasEmptyWorkerId()
    {
        // Arrange
        var command = CreateCommand("update", WorkerId.Empty, new Dictionary<string, string>
        {
            {"ProviderTarget", "test/repo"}
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("WorkerId is required for Update command");
    }

    [Fact]
    public async Task Handle_Should_ReturnWorkerNotFoundError_When_WorkerWithSpecifiedIdDoesNotExist()
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var command = CreateCommand("update", workerId, new Dictionary<string, string>
        {
            {"ProviderTarget", "test/repo"}
        });

        _mockRepo.Setup(r => r.Get(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkerConfiguration?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Worker with Id {workerId} not found");
    }

    [Fact]
    public async Task Handle_Should_ReturnParsingError_When_UpdateCommandHasNoParameters()
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var command = CreateCommand("update", workerId, new Dictionary<string, string>());
        var existingConfig = CreateWorkerConfiguration(workerId);

        _mockRepo.Setup(r => r.Get(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("Update command must have at least one parameter");
    }

    [Fact]
    public async Task Handle_Should_ReturnParsingError_When_UpdateCommandHasInvalidParameter()
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var command = CreateCommand("update", workerId, new Dictionary<string, string>
        {
            {"ProviderTarget", "valid/repo"},
            {"InvalidParameter", "value"}
        });
        var existingConfig = CreateWorkerConfiguration(workerId);

        _mockRepo.Setup(r => r.Get(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("Invalid parameter");
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_ValidatorReturnsFailure()
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var command = CreateCommand("update", workerId, new Dictionary<string, string>
        {
            {"ProviderTarget", "test/repo"}
        });
        var existingConfig = CreateWorkerConfiguration(workerId);
        var validationError = "Configuration validation failed";

        _mockRepo.Setup(r => r.Get(workerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);
        _mockValidator.Setup(v => v.Validate(It.IsAny<WorkerConfiguration>()))
            .Returns(ValidationResult.Fail(validationError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(validationError);
    }

    [Fact]
    public async Task Handle_Should_UpdateWorkerWithAllParameters_When_AllOperationsSucceed()
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var newProviderTarget = "new/repo";
        var newSchedule = "0 */10 * * * *";
        var newFilter = "state == 'closed'";
        var newClientTarget = "@updated";
        var newTemplate = "Updated template";

        var command = CreateCommand("update", workerId, new Dictionary<string, string>
        {
            {"ProviderTarget", newProviderTarget},
            {"Schedule", newSchedule},
            {"Filter", newFilter},
            {"ClientTarget", newClientTarget},
            {"Template", newTemplate}
        });
        var existingConfig = CreateWorkerConfiguration(workerId);

        SetupSuccessfulUpdate(existingConfig);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Worker {workerId} has been updated");

        VerifyRepositoryUpdateCalledWith(updatedConfig =>
            updatedConfig.ProviderInfo.Target == newProviderTarget &&
            updatedConfig.Schedule == newSchedule &&
            updatedConfig.Filter == newFilter &&
            updatedConfig.ClientInfo.Target == newClientTarget &&
            updatedConfig.Template == newTemplate);
    }

    [Fact]
    public async Task Handle_Should_PreserveExistingValues_When_UpdateCommandOmitsParameters()
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var newProviderTarget = "new/repo";
        var command = CreateCommand("update", workerId, new Dictionary<string, string>
        {
            {"ProviderTarget", newProviderTarget}
        });
        var existingConfig = CreateWorkerConfiguration(workerId);

        SetupSuccessfulUpdate(existingConfig);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Worker {workerId} has been updated");

        VerifyRepositoryUpdateCalledWith(updatedConfig =>
            updatedConfig.ProviderInfo.Target == newProviderTarget &&
            updatedConfig.Schedule == existingConfig.Schedule &&
            updatedConfig.Filter == existingConfig.Filter &&
            updatedConfig.ClientInfo.Target == existingConfig.ClientInfo.Target &&
            updatedConfig.Template == existingConfig.Template);
    }

    [Theory]
    [InlineData("ProviderTarget", TestDisplayName = "Provider target")]
    [InlineData("Schedule", TestDisplayName = "Schedule")]
    [InlineData("Filter", TestDisplayName = "Filter")]
    [InlineData("ClientTarget", TestDisplayName = "Client target")]
    [InlineData("Template", TestDisplayName = "Template")]
    public async Task Handle_Should_UseNewValueWhenFieldIsUpdated_When_UpdateProvided(string fieldName)
    {
        // Arrange
        var workerId = new WorkerId(Faker.Random.Long(1, 999999));
        var newValue = Faker.Lorem.Sentence();
        var command = CreateCommand("update", workerId, new Dictionary<string, string>
        {
            {fieldName, newValue}
        });
        var existingConfig = CreateWorkerConfiguration(workerId);

        SetupSuccessfulUpdate(existingConfig);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Worker {workerId} has been updated");

        VerifyRepositoryUpdateCalledWith(updatedConfig => GetConfigFieldValue(updatedConfig, fieldName) == newValue);
    }

    private Command CreateCommand(string name, WorkerId workerId = default, Dictionary<string, string>? parameters = null)
    {
        return new Command(name, workerId, parameters ?? new Dictionary<string, string>());
    }

    private WorkerConfiguration CreateWorkerConfiguration(WorkerId workerId)
    {
        var providerInfo = new ProviderInfo(ProviderType.Github, "original/repo");
        var clientInfo = new ClientInfo(ClientType.Telegram, "@original");

        return new WorkerConfiguration(
            workerId,
            providerInfo,
            "0 * * * * *",
            "state == 'open'",
            clientInfo,
            "Original template");
    }

    private void SetupSuccessfulUpdate(WorkerConfiguration existingConfig)
    {
        _mockRepo.Setup(r => r.Get(It.IsAny<WorkerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConfig);

        _mockValidator.Setup(v => v.Validate(It.IsAny<WorkerConfiguration>()))
            .Returns(ValidationResult.Success);

        _mockTransactionProvider.Setup(t => t.CreateTransaction(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockTransaction.Object);

        _mockRepo.Setup(r => r.Update(It.IsAny<WorkerConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockTransaction.Setup(t => t.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockScheduler.Setup(s => s.Schedule(It.IsAny<string>(), It.IsAny<WorkerId>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyRepositoryUpdateCalledWith(Func<WorkerConfiguration, bool> predicate)
    {
        _mockRepo.Verify(
            r => r.Update(
                It.Is<WorkerConfiguration>(c => predicate(c)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private string? GetConfigFieldValue(WorkerConfiguration config, string fieldName)
    {
        return fieldName switch
        {
            "ProviderTarget" => config.ProviderInfo.Target,
            "Schedule" => config.Schedule,
            "Filter" => config.Filter,
            "ClientTarget" => config.ClientInfo.Target,
            "Template" => config.Template,
            _ => null
        };
    }
}