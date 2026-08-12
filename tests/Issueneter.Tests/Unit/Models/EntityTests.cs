using FluentAssertions;
using Issueneter.Domain.Attributes;
using Issueneter.Domain.Models;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Models;

public class EntityTests : TestBase
{
    [Fact]
    public void GetFields_Should_ReturnAllPublicInstanceProperties()
    {
        // Arrange
        var entity = new EntityFieldsFixtureEntity();

        // Act
        var fields = entity.GetFields();

        // Assert
        fields.Select(field => field.Name).Should().BeEquivalentTo(
            [nameof(EntityFieldsFixtureEntity.DescribedProperty), nameof(EntityFieldsFixtureEntity.UndescribedProperty)],
            because: "GetFields should return every public instance property of the entity");
    }

    [Fact]
    public void GetFields_Should_ReturnNullDescription_When_FieldDescriptionAttributeIsAbsent()
    {
        // Arrange
        var entity = new EntityFieldsFixtureEntity();

        // Act
        var fields = entity.GetFields();

        // Assert
        var field = fields.Single(field => field.Name == nameof(EntityFieldsFixtureEntity.UndescribedProperty));
        field.Description.Should().BeNull(because: "properties without FieldDescriptionAttribute should have no description");
    }

    [Fact]
    public void GetFields_Should_ReturnAttributeDescription_When_FieldDescriptionAttributeIsPresent()
    {
        // Arrange
        var entity = new EntityFieldsFixtureEntity();

        // Act
        var fields = entity.GetFields();

        // Assert
        var field = fields.Single(field => field.Name == nameof(EntityFieldsFixtureEntity.DescribedProperty));
        field.Description.Should().Be("A described property", because: "GetFields should read the description from FieldDescriptionAttribute");
    }

    [Fact]
    public void GetFields_Should_ReturnPropertyType()
    {
        // Arrange
        var entity = new EntityFieldsFixtureEntity();

        // Act
        var fields = entity.GetFields();

        // Assert
        var field = fields.Single(field => field.Name == nameof(EntityFieldsFixtureEntity.DescribedProperty));
        field.PropertyType.Should().Be(typeof(string), because: "GetFields should report the CLR property type");
    }

    private class EntityFieldsFixtureEntity : Entity
    {
        [FieldDescription("A described property")]
        public string DescribedProperty { get; set; } = string.Empty;

        public int UndescribedProperty { get; set; }
    }
}
