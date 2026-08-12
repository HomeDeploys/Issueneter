using FluentAssertions;
using Issueneter.Application.Commands;
using Issueneter.Domain.Attributes;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class HelpFormatterTests : TestBase
{
    [Fact]
    public void FormatCommandList_Should_IncludeNameAndDescription_ForEveryHandler()
    {
        // Arrange
        var create = CreateHandler("create", "Creates a worker");
        var get = CreateHandler("get", "Gets a worker");

        // Act
        var result = HelpFormatter.FormatCommandList([create.Object, get.Object]);

        // Assert
        result.Should().Contain("- create: Creates a worker", because: "the list should include every handler's name and description");
        result.Should().Contain("- get: Gets a worker", because: "the list should include every handler's name and description");
    }

    [Fact]
    public void FormatCommandDetail_Should_IncludeNameDescriptionExample_When_Called()
    {
        // Arrange
        var handler = CreateHandler("create", "Creates a worker", example: "create\nProviderType: Github");

        // Act
        var result = HelpFormatter.FormatCommandDetail(handler.Object);

        // Assert
        result.Should().Contain("create", because: "detail should include the command name");
        result.Should().Contain("Creates a worker", because: "detail should include the command description");
        result.Should().Contain("create\nProviderType: Github", because: "detail should include the usage example");
    }

    [Fact]
    public void FormatCommandDetail_Should_IncludeArgumentNameRequirednessAndDescription_When_ArgumentsArePresent()
    {
        // Arrange
        var handler = CreateHandler(
            "create",
            "Creates a worker",
            arguments:
            [
                new CommandArgumentInfo("ProviderType", "Provider type, e.g. Github", IsRequired: true)
            ]);

        // Act
        var result = HelpFormatter.FormatCommandDetail(handler.Object);

        // Assert
        result.Should().Contain("ProviderType", because: "detail should list the argument name");
        result.Should().Contain("required", because: "detail should indicate the argument is required");
        result.Should().Contain("Provider type, e.g. Github", because: "detail should include the argument description");
    }

    [Fact]
    public void FormatCommandDetail_Should_IndicateNoArguments_When_ArgumentsAreEmpty()
    {
        // Arrange
        var handler = CreateHandler("get", "Gets a worker", arguments: []);

        // Act
        var result = HelpFormatter.FormatCommandDetail(handler.Object);

        // Assert
        result.Should().Contain("none", because: "detail should indicate there are no arguments");
    }

    [Fact]
    public void FormatProviderList_Should_IncludeTypeAndDescription_ForEveryProvider()
    {
        // Arrange
        var provider = CreateProvider(ProviderType.Github, "Polls Github issues");

        // Act
        var result = HelpFormatter.FormatProviderList([provider.Object]);

        // Assert
        result.Should().Contain("- Github: Polls Github issues", because: "the list should include every provider's type and description");
    }

    [Fact]
    public void FormatProviderDetail_Should_IncludeTargetFormatAndFields_When_Called()
    {
        // Arrange
        var provider = CreateProvider(ProviderType.Github, "Polls Github issues", targetFormat: "<owner>/<repo>");
        provider.Setup(p => p.GetSample()).Returns(new FormatterFixtureEntity());

        // Act
        var result = HelpFormatter.FormatProviderDetail(provider.Object);

        // Assert
        result.Should().Contain("Github", because: "detail should include the provider type");
        result.Should().Contain("Polls Github issues", because: "detail should include the provider description");
        result.Should().Contain("<owner>/<repo>", because: "detail should include the target format description");
        result.Should().Contain("DescribedProperty", because: "detail should list entity field names");
        result.Should().Contain("A described property", because: "detail should list field descriptions when present");
        result.Should().Contain("UndescribedProperty", because: "detail should list fields even without a description");
    }

    [Fact]
    public void FormatProviderDetail_Should_UseFriendlyGenericTypeName_When_FieldTypeIsGeneric()
    {
        // Arrange
        var provider = CreateProvider(ProviderType.Github, "Polls Github issues");
        provider.Setup(p => p.GetSample()).Returns(new FormatterFixtureEntity());

        // Act
        var result = HelpFormatter.FormatProviderDetail(provider.Object);

        // Assert
        result.Should().Contain("IReadOnlyCollection<String>", because: "generic field types should be rendered friendly, not as CLR names like IReadOnlyCollection`1");
        result.Should().NotContain("`1", because: "friendly type names should not leak CLR backtick-arity notation");
    }

    [Fact]
    public void FormatClientList_Should_IncludeTypeAndDescription_ForEveryClient()
    {
        // Arrange
        var client = CreateClient(ClientType.Telegram, "Sends Telegram messages");

        // Act
        var result = HelpFormatter.FormatClientList([client.Object]);

        // Assert
        result.Should().Contain("- Telegram: Sends Telegram messages", because: "the list should include every client's type and description");
    }

    [Fact]
    public void FormatClientDetail_Should_IncludeTypeDescriptionAndTargetFormat_When_Called()
    {
        // Arrange
        var client = CreateClient(ClientType.Telegram, "Sends Telegram messages", targetFormat: "<chatId> or <chatId>/<threadId>");

        // Act
        var result = HelpFormatter.FormatClientDetail(client.Object);

        // Assert
        result.Should().Contain("Telegram", because: "detail should include the client type");
        result.Should().Contain("Sends Telegram messages", because: "detail should include the client description");
        result.Should().Contain("<chatId> or <chatId>/<threadId>", because: "detail should include the target format description");
    }

    private static Mock<ICommandHandler> CreateHandler(
        string name,
        string description,
        string example = "example",
        IReadOnlyCollection<CommandArgumentInfo>? arguments = null)
    {
        var handler = new Mock<ICommandHandler>();
        handler.Setup(h => h.Name).Returns(name);
        handler.Setup(h => h.Description).Returns(description);
        handler.Setup(h => h.Example).Returns(example);
        handler.Setup(h => h.Arguments).Returns(arguments ?? []);
        return handler;
    }

    private static Mock<IEntityProvider> CreateProvider(ProviderType type, string description, string targetFormat = "target")
    {
        var provider = new Mock<IEntityProvider>();
        provider.Setup(p => p.Type).Returns(type);
        provider.Setup(p => p.Description).Returns(description);
        provider.Setup(p => p.TargetFormatDescription).Returns(targetFormat);
        return provider;
    }

    private static Mock<IClient> CreateClient(ClientType type, string description, string targetFormat = "target")
    {
        var client = new Mock<IClient>();
        client.Setup(c => c.Type).Returns(type);
        client.Setup(c => c.Description).Returns(description);
        client.Setup(c => c.TargetFormatDescription).Returns(targetFormat);
        return client;
    }

    private class FormatterFixtureEntity : Entity
    {
        [FieldDescription("A described property")]
        public IReadOnlyCollection<string> DescribedProperty { get; set; } = [];

        public int UndescribedProperty { get; set; }
    }
}
