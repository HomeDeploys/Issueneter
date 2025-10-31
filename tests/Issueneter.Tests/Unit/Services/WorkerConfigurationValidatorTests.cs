using FluentAssertions;
using Issueneter.Application.Services;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Helpers;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Services;

public class WorkerConfigurationValidatorTests : TestBase
{
    private readonly Mock<IProviderFactory> _mockProviderFactory;
    private readonly Mock<IClientFactory> _mockClientFactory;
    private readonly Mock<IScheduler> _mockScheduler;
    private readonly Mock<IMessageFormatter> _mockMessageFormatter;
    private readonly Mock<IFilterParser> _mockFilterParser;
    private readonly WorkerConfigurationValidator _validator;

    public WorkerConfigurationValidatorTests()
    {
        _mockProviderFactory = new Mock<IProviderFactory>();
        _mockClientFactory = new Mock<IClientFactory>();
        _mockScheduler = new Mock<IScheduler>();
        _mockMessageFormatter = new Mock<IMessageFormatter>();
        _mockFilterParser = new Mock<IFilterParser>();

        _validator = new WorkerConfigurationValidator(
            _mockProviderFactory.Object,
            _mockClientFactory.Object,
            _mockScheduler.Object,
            _mockMessageFormatter.Object,
            _mockFilterParser.Object);
    }

    [Fact]
    public void Validate_Should_ReturnSuccess_When_AllValidationsPass()
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        var sampleEntity = new TestEntity { StringProperty = "test" };
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);
        mockProvider.Setup(p => p.GetSample())
            .Returns(sampleEntity);

        var mockClient = new Mock<IClient>();
        mockClient.Setup(c => c.Validate(clientTarget))
            .Returns(ValidationResult.Success);

        var mockFilter = new Mock<IFilter>();
        mockFilter.Setup(f => f.IsValid(sampleEntity))
            .Returns(true);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns(mockClient.Object);
        _mockScheduler.Setup(s => s.Validate(schedule))
            .Returns(ValidationResult.Success);
        _mockFilterParser.Setup(fp => fp.Parse(filter))
            .Returns(ParseResult<IFilter>.Success(mockFilter.Object));
        _mockMessageFormatter.Setup(mf => mf.Validate(template, sampleEntity))
            .Returns(ValidationResult.Success);

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "All validations passed");
        result.Error.Should().BeEmpty(because: "No error should be present on success");
    }

    [Fact]
    public void Validate_Should_ReturnFail_When_ProviderNotFound()
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns((IEntityProvider?)null);

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Provider not found");
        result.Error.Should().NotBeEmpty(because: "Error message should be present");
        result.Error.Should().Contain("No provider found", because: "Error should indicate provider not found");
        result.Error.Should().Contain(providerType.ToString(), because: "Error should contain provider type");
    }

    [Theory]
    [InlineData("Invalid target format", TestDisplayName = "SingleWordError")]
    [InlineData("Target does not exist on the server", TestDisplayName = "MultipleWordsError")]
    public void Validate_Should_ReturnFail_When_ProviderTargetValidationFails(string errorMessage)
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "invalid-target";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Fail(errorMessage));

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Provider target validation failed");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("Invalid provider target", because: "Error should indicate invalid provider target");
        result.Error.Should().Contain(errorMessage, because: "Error should contain the validation error message");
    }

    [Fact]
    public void Validate_Should_ReturnFail_When_ClientNotFound()
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns((IClient?)null);

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Client not found");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("No client found", because: "Error should indicate client not found");
        result.Error.Should().Contain(clientType.ToString(), because: "Error should contain client type");
    }

    [Theory]
    [InlineData("Invalid channel ID", TestDisplayName = "InvalidChannelId")]
    [InlineData("Authentication token is expired", TestDisplayName = "ExpiredToken")]
    public void Validate_Should_ReturnFail_When_ClientTargetValidationFails(string errorMessage)
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "invalid-channel";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);

        var mockClient = new Mock<IClient>();
        mockClient.Setup(c => c.Validate(clientTarget))
            .Returns(ValidationResult.Fail(errorMessage));

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns(mockClient.Object);

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Client target validation failed");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("Invalid client target", because: "Error should indicate invalid client target");
        result.Error.Should().Contain(errorMessage, because: "Error should contain the validation error message");
    }

    [Theory]
    [InlineData("Invalid cron expression", TestDisplayName = "InvalidCronExpression")]
    [InlineData("Schedule format not supported", TestDisplayName = "UnsupportedFormat")]
    public void Validate_Should_ReturnFail_When_ScheduleValidationFails(string errorMessage)
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "invalid-schedule";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);

        var mockClient = new Mock<IClient>();
        mockClient.Setup(c => c.Validate(clientTarget))
            .Returns(ValidationResult.Success);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns(mockClient.Object);
        _mockScheduler.Setup(s => s.Validate(schedule))
            .Returns(ValidationResult.Fail(errorMessage));

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Schedule validation failed");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("Invalid schedule", because: "Error should indicate invalid schedule");
        result.Error.Should().Contain(errorMessage, because: "Error should contain the validation error message");
    }

    [Theory]
    [InlineData("Unexpected token at position 5", TestDisplayName = "UnexpectedToken")]
    [InlineData("Missing closing parenthesis", TestDisplayName = "SyntaxError")]
    public void Validate_Should_ReturnFail_When_FilterParsingFails(string errorMessage)
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "invalid filter syntax (";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        var sampleEntity = new TestEntity { StringProperty = "test" };
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);
        mockProvider.Setup(p => p.GetSample())
            .Returns(sampleEntity);

        var mockClient = new Mock<IClient>();
        mockClient.Setup(c => c.Validate(clientTarget))
            .Returns(ValidationResult.Success);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns(mockClient.Object);
        _mockScheduler.Setup(s => s.Validate(schedule))
            .Returns(ValidationResult.Success);
        _mockFilterParser.Setup(fp => fp.Parse(filter))
            .Returns(ParseResult<IFilter>.Fail(errorMessage));

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Filter parsing failed");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("Invalid filter format", because: "Error should indicate invalid filter format");
        result.Error.Should().Contain(errorMessage, because: "Error should contain the parsing error message");
    }

    [Fact]
    public void Validate_Should_ReturnFail_When_FilterIsNotValidForEntity()
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        var sampleEntity = new TestEntity { StringProperty = "test" };
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);
        mockProvider.Setup(p => p.GetSample())
            .Returns(sampleEntity);

        var mockClient = new Mock<IClient>();
        mockClient.Setup(c => c.Validate(clientTarget))
            .Returns(ValidationResult.Success);

        var mockFilter = new Mock<IFilter>();
        mockFilter.Setup(f => f.IsValid(sampleEntity))
            .Returns(false);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns(mockClient.Object);
        _mockScheduler.Setup(s => s.Validate(schedule))
            .Returns(ValidationResult.Success);
        _mockFilterParser.Setup(fp => fp.Parse(filter))
            .Returns(ParseResult<IFilter>.Success(mockFilter.Object));

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Filter is not valid for entity type");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("Filter is not valid for this type of entity", 
            because: "Error should indicate filter is incompatible with entity type");
    }

    [Theory]
    [InlineData("Template contains invalid placeholder {UnknownField}", TestDisplayName = "InvalidPlaceholder")]
    [InlineData("Template is too long, maximum length is 4096 characters", TestDisplayName = "TemplateTooLong")]
    public void Validate_Should_ReturnFail_When_TemplateValidationFails(string errorMessage)
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Invalid: {UnknownField}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        var mockProvider = new Mock<IEntityProvider>();
        var sampleEntity = new TestEntity { StringProperty = "test" };
        mockProvider.Setup(p => p.Validate(providerTarget))
            .Returns(ValidationResult.Success);
        mockProvider.Setup(p => p.GetSample())
            .Returns(sampleEntity);

        var mockClient = new Mock<IClient>();
        mockClient.Setup(c => c.Validate(clientTarget))
            .Returns(ValidationResult.Success);

        var mockFilter = new Mock<IFilter>();
        mockFilter.Setup(f => f.IsValid(sampleEntity))
            .Returns(true);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns(mockProvider.Object);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns(mockClient.Object);
        _mockScheduler.Setup(s => s.Validate(schedule))
            .Returns(ValidationResult.Success);
        _mockFilterParser.Setup(fp => fp.Parse(filter))
            .Returns(ParseResult<IFilter>.Success(mockFilter.Object));
        _mockMessageFormatter.Setup(mf => mf.Validate(template, sampleEntity))
            .Returns(ValidationResult.Fail(errorMessage));

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Template validation failed");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("Invalid template", because: "Error should indicate invalid template");
        result.Error.Should().Contain(errorMessage, because: "Error should contain the validation error message");
    }

    [Fact]
    public void Validate_Should_CheckValidationsInCorrectOrder_When_MultipleValidationsFail()
    {
        // Arrange
        var providerId = new WorkerId(Faker.Random.Long(1));
        var providerType = ProviderType.Github;
        var providerTarget = "owner/repo";
        var clientType = ClientType.Telegram;
        var clientTarget = "123456789";
        var schedule = "0 9 * * *";
        var filter = "type = 'issue'";
        var template = "Issue: {Title}";

        var configuration = new WorkerConfiguration(
            providerId,
            new ProviderInfo(providerType, providerTarget),
            schedule,
            filter,
            new ClientInfo(clientType, clientTarget),
            template);

        _mockProviderFactory.Setup(f => f.Get(providerType))
            .Returns((IEntityProvider?)null);
        _mockClientFactory.Setup(f => f.Get(clientType))
            .Returns((IClient?)null);

        // Act
        var result = _validator.Validate(configuration);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("provider", because: "Provider validation should be checked first");
        result.Error.Should().NotContain("client", because: "Should not reach client validation if provider fails");
    }
}