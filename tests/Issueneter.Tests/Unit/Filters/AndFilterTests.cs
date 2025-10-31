using FluentAssertions;
using Issueneter.Application.Parser.Binary;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;
using Issueneter.Tests.Helpers;
using Issueneter.Tests.Setup;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Filters;

public class AndFilterTests : TestBase
{
    [Fact]
    public void IsValid_Should_ReturnTrue_When_AllFiltersAreValid()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(true);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(true);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeTrue(because: "AndFilter should be valid when all sub-filters are valid");
        mockFilter1.Verify(f => f.IsValid(entity), Times.Once);
        mockFilter2.Verify(f => f.IsValid(entity), Times.Once);
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_FirstFilterIsInvalid()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(false);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(true);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "AndFilter should be invalid when at least one sub-filter is invalid");
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_LastFilterIsInvalid()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(true);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(false);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "AndFilter should be invalid when any sub-filter is invalid");
    }

    [Fact]
    public void IsValid_Should_ReturnFalse_When_MultipleFiltersAreInvalid()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(false);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(false);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeFalse(because: "AndFilter should be invalid when multiple sub-filters are invalid");
    }

    [Fact]
    public void IsApplicable_Should_ReturnTrue_When_AllFiltersAreApplicable()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(true);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(true);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: "AndFilter should be applicable when all sub-filters are applicable");
        mockFilter1.Verify(f => f.IsApplicable(entity), Times.Once);
        mockFilter2.Verify(f => f.IsApplicable(entity), Times.Once);
    }

    [Fact]
    public void IsApplicable_Should_ReturnFalse_When_FirstFilterIsNotApplicable()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(false);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(true);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: "AndFilter should not be applicable when at least one sub-filter is not applicable");
    }

    [Fact]
    public void IsApplicable_Should_ReturnFalse_When_LastFilterIsNotApplicable()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(true);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(false);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: "AndFilter should not be applicable when any sub-filter is not applicable");
    }

    [Fact]
    public void IsApplicable_Should_ReturnFalse_When_MultipleFiltersAreNotApplicable()
    {
        // Arrange
        var mockFilter1 = new Mock<IFilter>();
        mockFilter1.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(false);
        
        var mockFilter2 = new Mock<IFilter>();
        mockFilter2.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(false);

        var filter = new AndFilter(new[] { mockFilter1.Object, mockFilter2.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeFalse(because: "AndFilter should not be applicable when multiple sub-filters are not applicable");
    }

    [Fact]
    public void IsValid_Should_ReturnTrue_When_FilterCollectionHasOneValidFilter()
    {
        // Arrange
        var mockFilter = new Mock<IFilter>();
        mockFilter.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(true);

        var filter = new AndFilter(new[] { mockFilter.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeTrue(because: "AndFilter should be valid when single sub-filter is valid");
    }

    [Fact]
    public void IsApplicable_Should_ReturnTrue_When_FilterCollectionHasOneApplicableFilter()
    {
        // Arrange
        var mockFilter = new Mock<IFilter>();
        mockFilter.Setup(f => f.IsApplicable(It.IsAny<Entity>())).Returns(true);

        var filter = new AndFilter(new[] { mockFilter.Object });
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: "AndFilter should be applicable when single sub-filter is applicable");
    }

    [Fact]
    public void IsValid_Should_ReturnTrue_When_FilterCollectionIsEmpty()
    {
        // Arrange
        var filter = new AndFilter(Array.Empty<IFilter>());
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().BeTrue(because: "AndFilter with empty collection should be valid (all of empty set are valid)");
    }

    [Fact]
    public void IsApplicable_Should_ReturnTrue_When_FilterCollectionIsEmpty()
    {
        // Arrange
        var filter = new AndFilter(Array.Empty<IFilter>());
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsApplicable(entity);

        // Assert
        result.Should().BeTrue(because: "AndFilter with empty collection should be applicable (all of empty set are applicable)");
    }
}