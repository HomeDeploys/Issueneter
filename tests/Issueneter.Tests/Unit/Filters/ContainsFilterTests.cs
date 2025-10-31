using FluentAssertions;
using Issueneter.Application.Parser.Unary;
using Issueneter.Tests.Helpers;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Filters;

public class ContainsFilterTests : TestBase
{
    [Fact]
    public void IsValid_Should_ReturnTrue_When_PropertyIsCastableToEnumerableOfGenericType()
    {
        // Arrange
        var filter = new ContainsFilter<int>("IntArrayProperty", 1);
        var entity = new TestEntity { StringProperty = "test", IntArrayProperty = new[] { 1, 2, 3 } };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeTrue(because: "Filter should be valid when property is castable to IEnumerable<T>");
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_PropertyIsNotCastableToEnumerableOfGenericType()
    {
        // Arrange
        var filter = new ContainsFilter<int>("IntProperty", 1);
        var entity = new TestEntity { StringProperty = "test", IntProperty = 42 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should be invalid when property is not IEnumerable<T>");
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_PropertyDoesNotExist()
    {
        // Arrange
        var filter = new ContainsFilter<int>("NonExistentProperty", 1);
        var entity = new TestEntity { StringProperty = "test", IntArrayProperty = new[] { 1, 2 } };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should be invalid when property doesn't exist");
    }

    [Theory]
    [InlineData(1, TestDisplayName = "FirstElement")]
    [InlineData(2, TestDisplayName = "MiddleElement")]
    [InlineData(3, TestDisplayName = "LastElement")]
    public void IsApplicable_Should_ReturnTrue_When_CollectionContainsValue(int searchValue)
    {
        // Arrange
        var filter = new ContainsFilter<int>("IntArrayProperty", searchValue);
        var entity = new TestEntity { StringProperty = "test", IntArrayProperty = new[] { 1, 2, 3 } };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: $"Filter should be applicable when collection contains value {searchValue}");
    }

    [Theory]
    [InlineData(4, TestDisplayName = "ValueNotInCollection")]
    [InlineData(0, TestDisplayName = "ZeroNotInCollection")]
    [InlineData(-1, TestDisplayName = "NegativeValueNotInCollection")]
    public void IsApplicable_Should_ReturnFalse_When_CollectionDoesNotContainValue(int searchValue)
    {
        // Arrange
        var filter = new ContainsFilter<int>("IntArrayProperty", searchValue);
        var entity = new TestEntity { StringProperty = "test", IntArrayProperty = new[] { 1, 2, 3 } };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: $"Filter should not be applicable when collection doesn't contain value {searchValue}");
    }

    [Fact]
    public void IsApplicable_Should_ReturnFalse_When_CollectionIsNull()
    {
        // Arrange
        var filter = new ContainsFilter<int>("IntArrayProperty", 1);
        var entity = new TestEntity { StringProperty = "test", IntArrayProperty = null! };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should not be applicable when collection is null");
    }

    [Fact]
    public void IsApplicable_Should_ReturnFalse_When_CollectionIsEmpty()
    {
        // Arrange
        var filter = new ContainsFilter<int>("IntArrayProperty", 1);
        var entity = new TestEntity { StringProperty = "test", IntArrayProperty = Array.Empty<int>() };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: "Filter should not be applicable when collection is empty");
    }

    [Theory]
    [InlineData("apple", TestDisplayName = "StringInCollection")]
    [InlineData("APPLE", TestDisplayName = "UppercaseStringNotInCollection")]
    public void IsApplicable_Should_ReturnTrue_When_StringCollectionContainsValue(string searchValue)
    {
        // Arrange
        var filter = new ContainsFilter<string>("StringArrayProperty", searchValue);
        var entity = new TestEntity { StringProperty = "test", StringArrayProperty = new[] { "apple", "banana", "cherry" } };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().Be(searchValue == "apple", because: $"Filter should be applicable based on string match");
    }
}