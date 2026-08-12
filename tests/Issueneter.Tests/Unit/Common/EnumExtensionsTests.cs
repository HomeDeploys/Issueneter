using FluentAssertions;
using Issueneter.Common.Extensions;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Common;

public class EnumExtensionsTests : TestBase
{
    private enum SampleEnum
    {
        First = 0,
        Second = 1
    }

    [Theory]
    [InlineData("Second", TestDisplayName = "Exact case")]
    [InlineData("second", TestDisplayName = "Lower case")]
    [InlineData("SECOND", TestDisplayName = "Upper case")]
    [InlineData("SeCoNd", TestDisplayName = "Mixed case")]
    public void TryParseSafe_Should_ReturnTrue_When_ValueMatchesDefinedNameRegardlessOfCase(string value)
    {
        // Act
        var success = Enum.TryParseSafe<SampleEnum>(value, out var result);

        // Assert
        success.Should().BeTrue(because: "TryParseSafe should be case-insensitive for defined enum names");
        result.Should().Be(SampleEnum.Second, because: "the parsed value should match the enum member regardless of casing");
    }

    [Fact]
    public void TryParseSafe_Should_ReturnFalse_When_ValueIsNotADefinedName()
    {
        // Act
        var success = Enum.TryParseSafe<SampleEnum>("NotAMember", out _);

        // Assert
        success.Should().BeFalse(because: "values that don't match any enum member name should not parse");
    }

    [Fact]
    public void TryParseSafe_Should_ReturnFalse_When_ValueIsEmpty()
    {
        // Act
        var success = Enum.TryParseSafe<SampleEnum>(string.Empty, out _);

        // Assert
        success.Should().BeFalse(because: "an empty string is not a defined enum member name");
    }
}
