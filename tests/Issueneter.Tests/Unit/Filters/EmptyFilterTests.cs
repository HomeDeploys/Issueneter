using FluentAssertions;
using Issueneter.Application.Parser.Unary;
using Issueneter.Tests.Helpers;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Filters;

public class EmptyFilterTests : TestBase
{
    [Fact]
    public void IsValid_Should_ReturnTrue_When_EntityIsProvided()
    {
        // Arrange
        var filter = new EmptyFilter();
        var entity = new TestEntity { StringProperty = "test", IntProperty = 42 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeTrue(because: "EmptyFilter should be valid for any entity");
    }

    [Fact]
    public void IsApplicable_Should_ReturnTrue_When_EntityIsProvided()
    {
        // Arrange
        var filter = new EmptyFilter();
        var entity = new TestEntity { StringProperty = "test", IntProperty = 42 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: "EmptyFilter should be applicable to any entity");
    }
    
}