using FluentAssertions;
using Issueneter.Application.Commands;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class HelpCommandHandlerTests : TestBase
{
    [Theory]
    [InlineData("help", TestDisplayName = "LowercaseHelp")]
    [InlineData("HELP", TestDisplayName = "UppercaseHelp")]
    [InlineData("Help", TestDisplayName = "MixedCaseHelp")]
    public void CanHandle_Should_ReturnTrue_When_CommandNameIsHelp(string commandName)
    {
        // Arrange
        var handler = CreateHandler([], [], []);
        var command = new Command(commandName, WorkerId.Empty, new Dictionary<string, string>());

        // Act
        var result = handler.CanHandle(command);

        // Assert
        result.Should().BeTrue(because: $"handler should recognize '{commandName}' command case-insensitively");
    }

    [Theory]
    [InlineData("create", TestDisplayName = "CreateCommand")]
    [InlineData("get", TestDisplayName = "GetCommand")]
    [InlineData("unknown", TestDisplayName = "UnknownCommand")]
    public void CanHandle_Should_ReturnFalse_When_CommandNameIsNotHelp(string commandName)
    {
        // Arrange
        var handler = CreateHandler([], [], []);
        var command = new Command(commandName, WorkerId.Empty, new Dictionary<string, string>());

        // Act
        var result = handler.CanHandle(command);

        // Assert
        result.Should().BeFalse(because: $"handler should not recognize '{commandName}' command");
    }

    [Fact]
    public async Task Handle_Should_ReturnCommandList_When_NoTypeParameterIsProvided()
    {
        // Arrange
        var createHandler = Mock.Of<ICommandHandler>(h => h.Name == "create" && h.Description == "Creates a worker");
        var handler = CreateHandler([createHandler], [], []);
        var command = new Command("help", WorkerId.Empty, new Dictionary<string, string>());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatCommandList([createHandler]), because: "no Type parameter should list all registered commands");
    }

    [Fact]
    public async Task Handle_Should_ReturnCommandList_When_TypeIsCommandAndNameIsMissing()
    {
        // Arrange
        var createHandler = Mock.Of<ICommandHandler>(h => h.Name == "create" && h.Description == "Creates a worker");
        var handler = CreateHandler([createHandler], [], []);
        var command = new Command("help", WorkerId.Empty, new Dictionary<string, string> { ["Type"] = "command" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatCommandList([createHandler]), because: "Type=command without a Name should behave the same as bare help");
    }

    [Fact]
    public async Task Handle_Should_ReturnCommandDetail_When_TypeIsCommandAndNameMatchesExistingHandler()
    {
        // Arrange
        var createHandler = Mock.Of<ICommandHandler>(h =>
            h.Name == "create" && h.Description == "Creates a worker" && h.Example == "create\n..." && h.Arguments == Array.Empty<CommandArgumentInfo>());
        var handler = CreateHandler([createHandler], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "command", ["Name"] = "create" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatCommandDetail(createHandler), because: "a matching command name should return its detailed description");
    }

    [Theory]
    [InlineData("CREATE", TestDisplayName = "UppercaseName")]
    [InlineData("Create", TestDisplayName = "MixedCaseName")]
    public async Task Handle_Should_ReturnCommandDetail_When_NameMatchesCaseInsensitively(string name)
    {
        // Arrange
        var createHandler = Mock.Of<ICommandHandler>(h =>
            h.Name == "create" && h.Description == "Creates a worker" && h.Example == "create\n..." && h.Arguments == Array.Empty<CommandArgumentInfo>());
        var handler = CreateHandler([createHandler], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "command", ["Name"] = name });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatCommandDetail(createHandler), because: "command lookup by Name should be case-insensitive");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundMessage_When_TypeIsCommandAndNameDoesNotMatchAnyHandler()
    {
        // Arrange
        var createHandler = Mock.Of<ICommandHandler>(h => h.Name == "create");
        var handler = CreateHandler([createHandler], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "command", ["Name"] = "nope" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Command not found: nope", because: "an unknown command name should produce a not-found message");
    }

    [Fact]
    public async Task Handle_Should_ReturnProviderList_When_TypeIsProviderAndNameIsMissing()
    {
        // Arrange
        var provider = Mock.Of<IEntityProvider>(p => p.Type == ProviderType.Github && p.Description == "Polls Github issues");
        var handler = CreateHandler([], [provider], []);
        var command = new Command("help", WorkerId.Empty, new Dictionary<string, string> { ["Type"] = "provider" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatProviderList([provider]), because: "Type=provider without a Name should list all registered providers");
    }

    [Fact]
    public async Task Handle_Should_ReturnProviderDetail_When_TypeIsProviderAndNameMatchesRegisteredProvider()
    {
        // Arrange
        var provider = new Mock<IEntityProvider>();
        provider.Setup(p => p.Type).Returns(ProviderType.Github);
        provider.Setup(p => p.Description).Returns("Polls Github issues");
        provider.Setup(p => p.TargetFormatDescription).Returns("<owner>/<repo>");
        provider.Setup(p => p.GetSample()).Returns(new HelpCommandHandlerFixtureEntity());
        var handler = CreateHandler([], [provider.Object], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "provider", ["Name"] = "Github" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatProviderDetail(provider.Object), because: "a matching provider name should return its detailed description");
    }

    [Fact]
    public async Task Handle_Should_ReturnUnknownProviderMessage_When_NameIsNotAValidProviderType()
    {
        // Arrange
        var handler = CreateHandler([], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "provider", ["Name"] = "NotAProvider" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Unknown provider: NotAProvider", because: "a name that doesn't parse to a ProviderType should be reported as unknown");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundMessage_When_NameIsValidProviderTypeButNotRegistered()
    {
        // Arrange: ProviderType.Github is a valid enum value, but no matching IEntityProvider is injected.
        var handler = CreateHandler([], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "provider", ["Name"] = "Github" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Provider not found: Github", because: "a valid provider type without a matching registered provider should be reported as not found");
    }

    [Fact]
    public async Task Handle_Should_ReturnClientList_When_TypeIsClientAndNameIsMissing()
    {
        // Arrange
        var client = Mock.Of<IClient>(c => c.Type == ClientType.Telegram && c.Description == "Sends Telegram messages");
        var handler = CreateHandler([], [], [client]);
        var command = new Command("help", WorkerId.Empty, new Dictionary<string, string> { ["Type"] = "client" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatClientList([client]), because: "Type=client without a Name should list all registered clients");
    }

    [Fact]
    public async Task Handle_Should_ReturnClientDetail_When_TypeIsClientAndNameMatchesRegisteredClient()
    {
        // Arrange
        var client = Mock.Of<IClient>(c =>
            c.Type == ClientType.Telegram && c.Description == "Sends Telegram messages" && c.TargetFormatDescription == "<chatId>");
        var handler = CreateHandler([], [], [client]);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "client", ["Name"] = "Telegram" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(HelpFormatter.FormatClientDetail(client), because: "a matching client name should return its detailed description");
    }

    [Fact]
    public async Task Handle_Should_ReturnUnknownClientMessage_When_NameIsNotAValidClientType()
    {
        // Arrange
        var handler = CreateHandler([], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "client", ["Name"] = "NotAClient" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Unknown client: NotAClient", because: "a name that doesn't parse to a ClientType should be reported as unknown");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundMessage_When_NameIsValidClientTypeButNotRegistered()
    {
        // Arrange: ClientType.Telegram is a valid enum value, but no matching IClient is injected.
        var handler = CreateHandler([], [], []);
        var command = new Command(
            "help",
            WorkerId.Empty,
            new Dictionary<string, string> { ["Type"] = "client", ["Name"] = "Telegram" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("Client not found: Telegram", because: "a valid client type without a matching registered client should be reported as not found");
    }

    [Theory]
    [InlineData("bogus", TestDisplayName = "UnrecognizedWord")]
    [InlineData("commands", TestDisplayName = "PluralIsNotRecognized")]
    public async Task Handle_Should_ReturnUnknownTypeMessage_When_TypeIsUnrecognized(string type)
    {
        // Arrange
        var handler = CreateHandler([], [], []);
        var command = new Command("help", WorkerId.Empty, new Dictionary<string, string> { ["Type"] = type });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(
            $"Unknown help type: {type}. Expected 'command', 'provider' or 'client'.",
            because: "an unrecognized Type value should produce an error message");
    }

    [Fact]
    public void Metadata_Should_BeNonEmptyAndMatchHelpCommandContract()
    {
        // Arrange
        var handler = CreateHandler([], [], []);

        // Assert
        handler.Name.Should().Be("help", because: "help handler should be identified by the 'help' command name");
        handler.Description.Should().NotBeNullOrWhiteSpace(because: "handler must describe what the command does");
        handler.Example.Should().NotBeNullOrWhiteSpace(because: "handler must provide a usage example");
        handler.Arguments.Should().NotBeEmpty(because: "help command accepts optional Type/Name parameters");
    }

    private static HelpCommandHandler CreateHandler(
        IReadOnlyCollection<ICommandHandler> commandHandlers,
        IReadOnlyCollection<IEntityProvider> providers,
        IReadOnlyCollection<IClient> clients)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<ICommandHandler>)))
            .Returns(commandHandlers);

        return new HelpCommandHandler(serviceProvider.Object, providers, clients);
    }

    private class HelpCommandHandlerFixtureEntity : Entity
    {
    }
}
