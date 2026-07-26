using FluentAssertions;
using Issueneter.Application.Parser.Binary;
using Issueneter.Application.Parser.Unary;
using Issueneter.Domain.Interfaces.Filters;
using Issueneter.Domain.Models;
using Issueneter.Tests.Helpers;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Filters;

public class NotFilterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IsValid_Should_ReturnChildFilterValue(bool value)
    {
        // Arrange
        var mockFilter = new Mock<IFilter>();
        mockFilter.Setup(f => f.IsValid(It.IsAny<Entity>())).Returns(value);
        
        var filter = new NotFilter(mockFilter.Object);
        var entity = new TestEntity { StringProperty = "test", IntProperty = 10 };

        // Act
        var result = filter.IsValid(entity);

        // Assert
        result.Should().Be(value);
    }
}