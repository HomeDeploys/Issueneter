using FluentAssertions;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class CreateCommandHandlerTests : TestBase
{
    private readonly Mock<IWorkerConfigurationValidator> _mockValidator;
    private readonly Mock<IScheduler> _mockScheduler;
    private readonly Mock<IWorkerConfigurationRepo> _mockRepo;
    private readonly Mock<ITransactionProvider> _mockTransactionProvider;
    private readonly Mock<ITransaction> _mockTransaction;
    private readonly CreateCommandHandler _handler;

    public CreateCommandHandlerTests()
    {
        _mockValidator = new Mock<IWorkerConfigurationValidator>();
        _mockScheduler = new Mock<IScheduler>();
        _mockRepo = new Mock<IWorkerConfigurationRepo>();
        _mockTransactionProvider = new Mock<ITransactionProvider>();
        _mockTransaction = new Mock<ITransaction>();
        
        _handler = new CreateCommandHandler(
            _mockValidator.Object,
            _mockScheduler.Object,
            _mockRepo.Object,
            _mockTransactionProvider.Object);
    }

    [Theory]
    [InlineData("create", TestDisplayName = "Lowercase")]
    [InlineData("CREATE", TestDisplayName = "Uppercase")]
    [InlineData("Create", TestDisplayName = "Mixed case")]
    public void CanHandle_Should_ReturnTrue_When_CreateCommandProvided(string commandName)
    {
        // Arrange
        var command = CreateCommand(commandName);

        // Act
        var result = _handler.CanHandle(command);

        // Assert
        result.Should().BeTrue(because: "handler should recognize command");
    }

    [Fact]
    public void CanHandle_Should_ReturnFalse_When_RandomCommandProvided()
    {
        // Arrange
        var command = CreateCommand(Faker.Lorem.Word());

        // Act
        var result = _handler.CanHandle(command);

        // Assert
        result.Should().BeFalse(because: "handler should not recognize non-create commands");
    }

    [Fact]
    public async Task Handle_Should_ReturnMissingParameterError_When_CommandHasMissingRequiredParameters()
    {
        // Arrange
        var command = CreateCommand("create", new Dictionary<string, string>
        {
            {"ProviderType", "Github"},
            {"ProviderTarget", "test/repo"}
            // Missing other required parameters
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("Missing required parameter", because: "missing parameters should be reported");
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalidParameterError_When_CommandHasInvalidParameters()
    {
        // Arrange
        var command = CreateCommand("create", new Dictionary<string, string>
        {
            {"ProviderType", "Github"},
            {"ProviderTarget", "test/repo"},
            {"Schedule", "* * * * *"},
            {"Filter", "true"},
            {"ClientType", "Telegram"},
            {"ClientTarget", "@test"},
            {"Template", "Test message"},
            {"InvalidParameter", "value"}
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("Invalid parameter", because: "invalid parameters should be reported");
    }

    [Theory]
    [InlineData("Invalid", TestDisplayName = "Invalid provider")]
    [InlineData("", TestDisplayName = "Empty provider")]
    [InlineData("99999", TestDisplayName = "Numeric provider")]
    public async Task Handle_Should_ReturnInvalidProviderTypeError_When_InvalidProviderTypeProvided(string providerType)
    {
        // Arrange
        var command = CreateValidCommand(providerType: providerType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("Invalid provider type", because: "invalid provider type should return specific error");
    }

    [Theory]
    [InlineData("Invalid", TestDisplayName = "Invalid client")]
    [InlineData("", TestDisplayName = "Empty client")]
    [InlineData("99999", TestDisplayName = "Numeric client")]
    public async Task Handle_Should_ReturnInvalidProviderTypeError_When_InvalidClientTypeProvided(string clientType)
    {
        // Arrange
        var command = CreateValidCommand(clientType: clientType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain("Invalid client type", because: "invalid client type should return error message");
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_WorkerConfigurationValidatorReturnsFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var validationError = "Configuration validation failed";
        _mockValidator.Setup(v => v.Validate(It.IsAny<WorkerConfiguration>()))
            .Returns(ValidationResult.Fail(validationError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(validationError, because: "validation failure should return validation error message");
    }

    [Fact]
    public async Task Handle_Should_CallAllServicesWithValidValuesAndReturnWorkerCreatedMessage_When_AllOperationsSucceed()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedWorkerId = new WorkerId(123);
        SetupSuccessfulValidation();
        SetupSuccessfulRepositoryAndScheduler(expectedWorkerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be($"Worker {expectedWorkerId} has been created", because: "successful operations should return creation confirmation");
        
        // Verify all services were called
        _mockValidator.Verify(v => v.Validate(It.IsAny<WorkerConfiguration>()), Times.Once, "validator should be called");
        _mockTransactionProvider.Verify(t => t.CreateTransaction(It.IsAny<CancellationToken>()), Times.Once, "transaction should be created");
        _mockRepo.Verify(r => r.Create(It.IsAny<WorkerConfiguration>(), It.IsAny<CancellationToken>()), Times.Once, "repository should create worker");
        _mockTransaction.Verify(t => t.Commit(It.IsAny<CancellationToken>()), Times.Once, "transaction should be committed");
        _mockScheduler.Verify(s => s.Schedule(It.IsAny<string>(), expectedWorkerId), Times.Once, "scheduler should schedule worker");
    }

    private Command CreateCommand(string name, Dictionary<string, string>? parameters = null)
    {
        return new Command(name, WorkerId.Empty, parameters ?? new Dictionary<string, string>());
    }

    private Command CreateValidCommand(string providerType = "Github", string clientType = "Telegram")
    {
        return CreateCommand("create", new Dictionary<string, string>
        {
            {"ProviderType", providerType},
            {"ProviderTarget", Faker.Internet.Url()},
            {"Schedule", "0 */5 * * * *"},
            {"Filter", "state == 'open'"},
            {"ClientType", clientType},
            {"ClientTarget", Faker.Internet.UserName()},
            {"Template", Faker.Lorem.Sentence()}
        });
    }

    private void SetupSuccessfulValidation()
    {
        _mockValidator.Setup(v => v.Validate(It.IsAny<WorkerConfiguration>()))
            .Returns(ValidationResult.Success);
    }

    private void SetupSuccessfulRepositoryAndScheduler(WorkerId? workerId = null)
    {
        var expectedWorkerId = workerId ?? new WorkerId(Faker.Random.Long(1, 999999));
        
        _mockTransactionProvider.Setup(t => t.CreateTransaction(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockTransaction.Object);
        
        _mockRepo.Setup(r => r.Create(It.IsAny<WorkerConfiguration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWorkerId);
        
        _mockTransaction.Setup(t => t.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockScheduler.Setup(s => s.Schedule(It.IsAny<string>(), expectedWorkerId))
            .Returns(Task.CompletedTask);
    }
}