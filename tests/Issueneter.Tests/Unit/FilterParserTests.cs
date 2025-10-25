using FluentAssertions;
using Issueneter.Application.Parser;
using Issueneter.Application.Parser.Unary;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit;

public class FilterParserTests : TestBase
{
    private readonly FilterParser _filterParser = new FilterParser();

    [Fact]
    public void Parse_Should_ReturnEmptyFilterWhenInputIsEmpty()
    {
        // Arrange
        var emptyInput = "";

        // Act
        var result = _filterParser.Parse(emptyInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Entity.Should().BeOfType<EmptyFilter>();
    }

    [Fact]
    public void Parse_Should_ReturnEmptyFilterWhenInputIsWhitespace()
    {
        // Arrange
        var whitespaceInput = "   \t\n\r  ";

        // Act
        var result = _filterParser.Parse(whitespaceInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Entity.Should().BeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("equals(name, \"value\")", TestDisplayName  = "EqualsWithStringValue")]
    [InlineData("equals(field, 123)", TestDisplayName = "EqualsWithIntegerValue")]
    [InlineData("equals(property, 45.67)", TestDisplayName = "EqualsWithDoubleValue")]
    [InlineData("equals(status, -10)", TestDisplayName = "EqualsWithNegativeInteger")]
    [InlineData("equals(score, -15.5)", TestDisplayName = "EqualsWithNegativeDouble")]
    public void Parse_Should_SucceedWhenEqualsOperationIsSimple(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Filter should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
        result.Entity.Should().NotBeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("contains(title, \"search term\")", TestDisplayName = "ContainsWithStringValue")]
    [InlineData("contains(description, 42)", TestDisplayName = "ContainsWithIntegerValue")]
    [InlineData("contains(content, 3.14)", TestDisplayName = "ContainsWithDoubleValue")]
    public void Parse_Should_SucceedWhenContainsOperationIsSimple(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Filter should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
        result.Entity.Should().NotBeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("EQUALS(name, \"value\")", TestDisplayName = "UppercaseEquals")]
    [InlineData("equals(name, \"value\")", TestDisplayName = "LowercaseEquals")]
    [InlineData("Equals(name, \"value\")", TestDisplayName = "MixedCaseEquals")]
    [InlineData("CONTAINS(title, \"search\")", TestDisplayName = "UppercaseContains")]
    [InlineData("contains(title, \"search\")", TestDisplayName = "LowercaseContains")]
    [InlineData("Contains(title, \"search\")", TestDisplayName = "MixedCaseContains")]
    [InlineData("AND(equals(name, \"test\"), contains(title, \"title\"))", TestDisplayName = "UppercaseAnd")]
    [InlineData("and(equals(name, \"test\"), contains(title, \"title\"))", TestDisplayName = "LowercaseAnd")]
    [InlineData("And(equals(name, \"test\"), contains(title, \"title\"))", TestDisplayName = "MixedCaseAnd")]
    [InlineData("OR(equals(name, \"test\"), contains(title, \"title\"))", TestDisplayName = "UppercaseOr")]
    [InlineData("or(equals(name, \"test\"), contains(title, \"title\"))", TestDisplayName = "LowercaseOr")]
    [InlineData("Or(equals(name, \"test\"), contains(title, \"title\"))", TestDisplayName = "MixedCaseOr")]
    public void Parse_Should_WorkWhenOperatorsAreCaseInsensitive(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Filter should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
        result.Entity.Should().NotBeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("and(equals(name, \"test\"), contains(title, \"search\"))", TestDisplayName = "AndWithTwoFilters")]
    [InlineData("and(equals(status, \"active\"), contains(description, \"keyword\"), equals(type, 1))", TestDisplayName = "AndWithThreeFilters")]
    public void Parse_Should_SucceedWhenAndOperationHasMultipleFilters(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Filter should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
        result.Entity.Should().NotBeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("or(equals(name, \"test\"), contains(title, \"search\"))", TestDisplayName = "OrWithTwoFilters")]
    [InlineData("or(equals(status, \"active\"), contains(description, \"keyword\"), equals(type, 1))", TestDisplayName = "OrWithThreeFilters")]
    public void Parse_Should_SucceedWhenOrOperationHasMultipleFilters(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Filter should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
        result.Entity.Should().NotBeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("invalid syntax", TestDisplayName = "InvalidSyntax")]
    [InlineData("equals()", TestDisplayName = "MissingParameters")]
    [InlineData("equals(name)", TestDisplayName = "MissingValue")]
    [InlineData("equals(name, \"value\", extra)", TestDisplayName = "TooManyParameters")]
    [InlineData("and()", TestDisplayName = "EmptyAndOperation")]
    [InlineData("or(equals(name, \"test\"))", TestDisplayName = "OrWithSingleFilter")]
    [InlineData("equals(, \"value\")", TestDisplayName = "MissingFieldName")]
    [InlineData("equals(name, )", TestDisplayName = "MissingValue")]
    [InlineData("unknownop(name, \"value\")", TestDisplayName = "UnknownOperator")]
    [InlineData("equals(name \"value\")", TestDisplayName = "MissingComma")]
    [InlineData("equals[name, \"value\"]", TestDisplayName = "WrongBrackets")]
    [InlineData("equals(name, \"unclosed string)", TestDisplayName = "UnclosedString")]
    [InlineData("and(equals(name, \"test\"", TestDisplayName = "UnclosedParentheses")]
    public void Parse_Should_ReturnFailureWhenSyntaxIsInvalid(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeFalse(because: "Invalid syntax should return failure");
        result.Entity.Should().BeNull();
        result.Error.Should().NotBeNullOrEmpty(because: "Error message should be provided for invalid syntax");
    }

    [Theory]
    [InlineData("equals(field123, \"value\")", TestDisplayName = "AlphanumericFieldName")]
    [InlineData("contains(a1b2c3, 999)", TestDisplayName = "MixedAlphanumericField")]
    public void Parse_Should_SucceedWhenFieldNamesAreValid(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Valid field names should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
    }

    [Theory]
    [InlineData("contains(title, \"escaped \\\"quotes\\\" test\")", TestDisplayName = "EscapedQuotes")]
    [InlineData("equals(path, \"C:\\\\Program Files\\\\App\")", TestDisplayName = "EscapedBackslashes")]
    public void Parse_Should_SucceedWhenStringsAreEscaped(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Escaped strings should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
    }

    [Theory]
    [InlineData("and(or(equals(name, \"test\"), contains(title, \"search\")), equals(status, \"active\"))", TestDisplayName = "NestedAndOr")]
    [InlineData("or(and(equals(type, 1), contains(desc, \"keyword\")), equals(flag, \"true\"))", TestDisplayName = "NestedOrAnd")]
    public void Parse_Should_SucceedWhenOperationsAreNested(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Nested operations should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
        result.Entity.Should().NotBeOfType<EmptyFilter>();
    }

    [Theory]
    [InlineData("equals(name, 42.0)", TestDisplayName = "DoubleWithZeroDecimal")]
    [InlineData("contains(field, -0)", TestDisplayName = "NegativeZero")]
    [InlineData("equals(value, 0.123456)", TestDisplayName = "MultipleDecimals")]
    public void Parse_Should_SucceedWhenNumberFormatsAreVarious(string filter)
    {
        // Arrange
        // filter provided by test case

        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        result.IsSuccess.Should().BeTrue(because: $"Various number formats should parse successfully. Error: {result.Error}");
        result.Entity.Should().NotBeNull();
    }

    [Fact]
    public void Parse_Should_ReturnEmptyFilterWhenInputIsNull()
    {
        // Arrange
        string? nullInput = null;

        // Act
        var result = _filterParser.Parse(nullInput!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Entity.Should().BeOfType<EmptyFilter>();
    }
}