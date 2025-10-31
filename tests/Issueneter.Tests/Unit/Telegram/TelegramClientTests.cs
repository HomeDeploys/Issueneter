using FluentAssertions;
using Issueneter.Domain.Enums;
using Issueneter.Infrastructure.Telegram.Services;
using Issueneter.Tests.Setup;
using Moq;
using Telegram.Bot;
using Xunit;

namespace Issueneter.Tests.Unit.Telegram;

public class TelegramClientTests : TestBase
{
    private readonly TelegramClient _telegramClient;

    public TelegramClientTests()
    {
        _telegramClient = new TelegramClient(Mock.Of<ITelegramBotClient>());
    }

    [Fact]
    public void Type_Should_ReturnTelegramClientType()
    {
        _telegramClient.Type.Should().Be(ClientType.Telegram, because: "TelegramClient should always return Telegram client type");
    }

    [Theory]
    [InlineData("123456", TestDisplayName = "PositiveChatIdOnly")]
    [InlineData("-123456", TestDisplayName = "NegativeChatIdOnly")]
    [InlineData("123456/789", TestDisplayName = "PositiveChatIdWithPositiveThreadId")]
    [InlineData("-123456/789", TestDisplayName = "NegativeChatIdWithPositiveThreadId")]
    [InlineData("123456/-789", TestDisplayName = "PositiveChatIdWithNegativeThreadId")]
    [InlineData("-123456/-789", TestDisplayName = "NegativeChatIdWithNegativeThreadId")]
    public void Validate_Should_ReturnSuccess_When_TargetHasValidFormat(string target)
    {
        // Arrange
        // Act
        var result = _telegramClient.Validate(target);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Target format '{target}' should be valid");
    }

    [Theory]
    [InlineData("", TestDisplayName = "EmptyString")]
    [InlineData("invalid", TestDisplayName = "NonNumericValue")]
    [InlineData("123/abc", TestDisplayName = "NonNumericThreadId")]
    [InlineData("abc/123", TestDisplayName = "NonNumericChatId")]
    [InlineData("123/456/789", TestDisplayName = "MultipleSeparators")]
    [InlineData("123 456", TestDisplayName = "SpaceInsteadOfSlash")]
    [InlineData("123/", TestDisplayName = "MissingThreadId")]
    [InlineData("/123", TestDisplayName = "MissingChatId")]
    public void Validate_Should_ReturnFail_When_TargetHasInvalidFormat(string target)
    {
        // Arrange
        // Act
        var result = _telegramClient.Validate(target);

        // Assert
        result.IsSuccess.Should().BeFalse(because: $"Target format '{target}' should be invalid");
        result.Error.Should().NotBeEmpty(because: "Error message should be present");
        result.Error.Should().Contain("Telegram target must be in format", because: "Error message should describe expected format");
    }
}