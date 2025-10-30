using FluentAssertions;
using Issueneter.Application.Services;
using Issueneter.Domain.Models;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Services;

public class MessageFormatterTests : TestBase
{
    private readonly MessageFormatter _formatter = new();
    
    [Fact]
    public void Validate_Should_ReturnSuccess_When_TemplateIsValidWithoutPlaceholders()
    {
        // Arrange
        var template = "Hello World";
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = _formatter.Validate(template, entity);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "Template without placeholders should be valid");
        result.Error.Should().BeEmpty(because: "No error should be present on success");
    }

    [Theory]
    [InlineData("Hello {Name}", TestDisplayName = "SinglePlaceholder")]
    [InlineData("User {Name} has status {Status}", TestDisplayName = "MultiplePlaceholders")]
    [InlineData("Name: {Name}, Age: {Age}, City: {City}", TestDisplayName = "ThreePlaceholders")]
    public void Validate_Should_ReturnSuccess_When_TemplateHasValidPlaceholders(string template)
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = "30", City = "NYC", Status = "Active" };

        // Act
        var result = _formatter.Validate(template, entity);

        // Assert
        result.IsSuccess.Should().BeTrue(because: "Template with valid properties should pass validation");
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Validate_Should_ReturnFail_When_TemplateIsEmpty()
    {
        // Arrange
        var template = "";
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = _formatter.Validate(template, entity);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Empty template should fail validation");
        result.Error.Should().NotBeEmpty(because: "Error message should explain the failure");
        result.Error.Should().Contain("empty", because: "Error should mention empty template");
    }

    [Fact]
    public void Validate_Should_ReturnFail_When_TemplateIsNull()
    {
        // Arrange
        string? template = null;
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = _formatter.Validate(template!, entity);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Null template should fail validation");
        result.Error.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("{NonExistentProperty}", TestDisplayName = "SingleInvalidProperty")]
    [InlineData("Hello {Name} and {NonExistentProperty}", TestDisplayName = "MixedValidAndInvalidProperties")]
    [InlineData("{MissingProp1} {MissingProp2} {MissingProp3}", TestDisplayName = "MultipleInvalidProperties")]
    public void Validate_Should_ReturnFail_When_PropertyNotFound(string template)
    {
        // Arrange
        var entity = new TestEntity { Name = "John", Age = "30" };

        // Act
        var result = _formatter.Validate(template, entity);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Template with non-existent properties should fail");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("not found", because: "Error should indicate property not found");
    }

    [Fact]
    public void Validate_Should_ReturnFail_When_FirstOccurrenceOfInvalidPropertyIsFound()
    {
        // Arrange
        var template = "{Name} {MissingProp1} {MissingProp2}";
        var entity = new TestEntity { Name = "value" };

        // Act
        var result = _formatter.Validate(template, entity);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("MissingProp1", because: "Should report the first missing property");
    }

    [Fact]
    public void Format_Should_ReturnEmptyString_When_TemplateIsEmpty()
    {
        // Arrange
        var template = "";
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().BeEmpty(because: "Empty template should result in empty string");
    }

    [Fact]
    public void Format_Should_ReturnEmptyString_When_TemplateIsWhitespace()
    {
        // Arrange
        var template = "   \t\n  ";
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().BeEmpty(because: "Whitespace-only template should result in empty string");
    }

    [Fact]
    public void Format_Should_ReturnUnchangedTemplate_When_TemplateHasNoPlaceholders()
    {
        // Arrange
        var template = "Hello World";
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Hello World", because: "Template without placeholders should remain unchanged");
    }
    

    [Fact]
    public void Format_Should_ReplaceMultiplePlaceholders_When_TemplateHasThreePlaceholders()
    {
        // Arrange
        var template = "{Name}, {Age}, {City}";
        var entity = new TestEntity { Name = "John", Age = "30", City = "NYC" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("John, 30, NYC", because: "All placeholders should be replaced correctly");
    }

    [Fact]
    public void Format_Should_ReplaceWithEmptyString_When_PropertyValueIsNull()
    {
        // Arrange
        var template = "Hello {NullProperty}";
        var entity = new TestEntity { NullProperty = null };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Hello ", because: "Null property should be replaced with empty string");
    }

    [Fact]
    public void Format_Should_HandleRepeatedPlaceholders_When_SamePropertyUsedMultipleTimes()
    {
        // Arrange
        var template = "{Name} {Name} {Name}";
        var entity = new TestEntity { Name = "Echo" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Echo Echo Echo", because: "Repeated placeholders should all be replaced");
    }

    [Fact]
    public void Format_Should_ReplacePlaceholder_When_PropertyNameCaseMatches()
    {
        // Arrange
        var template = "{Name}";
        var entity = new TestEntity { Name = "Value" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Value", because: "Placeholder should be replaced when property name case matches");
    }

    [Theory]
    [InlineData("{name}", TestDisplayName = "LowercaseProperty")]
    [InlineData("{NAME}", TestDisplayName = "UppercaseProperty")]
    [InlineData("{NaMe}", TestDisplayName = "MixedCaseProperty")]
    public void Format_Should_ThrowException_When_PropertyNameCaseDoesNotMatch(string template)
    {
        // Arrange
        var entity = new TestEntity { Name = "Value" };

        // Act & Assert
        var action = () => _formatter.Format(template, entity);
        action.Should().Throw<ArgumentException>(because: "Property lookup is case-sensitive");
    }

    [Fact]
    public void Format_Should_HandleSpecialCharactersInPropertyValue_When_PropertyContainsSpecialChars()
    {
        // Arrange
        var template = "Path: {FilePath}";
        var entity = new TestEntity { FilePath = "C:\\Program Files\\App" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Path: C:\\Program Files\\App", because: "Special characters should be preserved");
    }

    [Fact]
    public void Format_Should_HandleNumericProperty_When_PropertyIsNumeric()
    {
        // Arrange
        var template = "Age: {Age}";
        var entity = new TestEntity { Age = "25" };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Age: 25", because: "Numeric properties should be formatted correctly");
    }

    [Fact]
    public void Format_Should_HandleEmptyPropertyValue_When_PropertyIsEmpty()
    {
        // Arrange
        var template = "Name: '{Name}'";
        var entity = new TestEntity { Name = string.Empty };

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Name: ''", because: "Empty string property should be preserved as empty");
    }
    
    [Fact]
    public void Format_Should_HandlePropertyValue_When_PropertyIsEnumerable()
    {
        // Arrange
        var template = "Value: {ListProperty}";
        var entity = new TestEntity { ListProperty = new []{"1", "2", "3"}};

        // Act
        var result = _formatter.Format(template, entity);

        // Assert
        result.Should().Be("Value: 1, 2, 3", because: "Enumerable properties should be formatted correctly");
    }

    /// <summary>
    /// Test entity for MessageFormatter tests with various property types.
    /// </summary>
    private class TestEntity : Entity
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? Age { get; set; }
        public string? City { get; set; }
        public string? NullProperty { get; set; }
        public string? FilePath { get; set; }
        
        public IReadOnlyCollection<string>? ListProperty { get; set; }
    }
}