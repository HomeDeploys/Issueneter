using FluentAssertions;
using Issueneter.Application.Parser.Unary;
using Issueneter.Tests.Helpers;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Filters;

public class EqualityFilterTests : TestBase
{
    [Fact]
    public void IsValid_Should_ReturnTrue_When_PropertyIsCastableToGenericType()
    {
        // Arrange
        var filter = new EqualityFilter<int>("IntProperty", 42);
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeTrue(because: "Filter should be valid when property is castable to the generic type");
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_PropertyIsNotCastableToGenericType()
    {
        // Arrange
        var filter = new EqualityFilter<IEnumerable<int>>("IntProperty", new[] { 1 });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 42 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should be invalid when property cannot be cast to the generic type");
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_PropertyDoesNotExist()
    {
        // Arrange
        var filter = new EqualityFilter<int>("NonExistentProperty", 42);
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should be invalid when property doesn't exist on the entity");
    }

    [Theory]
    [InlineData(42, 42, TestDisplayName = "SameIntegerValues")]
    [InlineData(0, 0, TestDisplayName = "BothZero")]
    [InlineData(-5, -5, TestDisplayName = "SameNegativeValues")]
    public void IsApplicable_Should_ReturnTrue_When_PropertyEqualsFilterValue(int entityValue, int filterValue)
    {
        // Arrange
        var filter = new EqualityFilter<int>("IntProperty", filterValue);
        var entity = new TestEntity { StringProperty = "test", IntProperty = entityValue };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: $"Filter should be applicable when entity value '{entityValue}' equals filter value '{filterValue}'");
    }

    [Theory]
    [InlineData(42, 10, TestDisplayName = "DifferentPositiveValues")]
    [InlineData(0, 1, TestDisplayName = "ZeroAndPositive")]
    [InlineData(-5, 5, TestDisplayName = "NegativeAndPositive")]
    public void IsApplicable_Should_ReturnFalse_When_PropertyNotEqualsFilterValue(int entityValue, int filterValue)
    {
        // Arrange
        var filter = new EqualityFilter<int>("IntProperty", filterValue);
        var entity = new TestEntity { StringProperty = "test", IntProperty = entityValue };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: $"Filter should not be applicable when entity value '{entityValue}' differs from filter value '{filterValue}'");
    }

    [Theory]
    [InlineData("test", "test", TestDisplayName = "SameStrings")]
    [InlineData("", "", TestDisplayName = "BothEmptyStrings")]
    [InlineData("TestString", "TestString", TestDisplayName = "SameMultiWordStrings")]
    public void IsApplicable_Should_ReturnTrue_When_StringPropertyEqualsFilterValue(string entityValue, string filterValue)
    {
        // Arrange
        var filter = new EqualityFilter<string>("StringProperty", filterValue);
        var entity = new TestEntity { StringProperty = entityValue, IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: $"Filter should be applicable when string value '{entityValue}' equals filter value '{filterValue}'");
    }

    [Theory]
    [InlineData("test", "other", TestDisplayName = "DifferentStrings")]
    [InlineData("", "test", TestDisplayName = "EmptyAndNonEmpty")]
    [InlineData("TEST", "test", TestDisplayName = "DifferentCaseStrings")]
    public void IsApplicable_Should_ReturnFalse_When_StringPropertyNotEqualsFilterValue(string entityValue, string filterValue)
    {
        // Arrange
        var filter = new EqualityFilter<string>("StringProperty", filterValue);
        var entity = new TestEntity { StringProperty = entityValue, IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: $"Filter should not be applicable when string value differs");
    }

    [Fact]
    public void IsApplicable_Should_ReturnFalse_When_FilterValueIsNull()
    {
        // Arrange
        var filter = new EqualityFilter<string?>("StringProperty", null);
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should not be applicable when filter value is null");
    }
}