using FluentAssertions;
using Issueneter.Application.Parser;
using Issueneter.Application.Parser.Unary;
using NUnit.Framework;

namespace Issueneter.Tests.Unit;

[TestFixture]
public class FilterParserTests
{
    private readonly FilterParser _filterParser = new FilterParser();

    [Test]
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

    [Test]
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

    [TestCase("equals(name, \"value\")", TestName = "EqualsWithStringValue")]
    [TestCase("equals(field, 123)", TestName = "EqualsWithIntegerValue")]
    [TestCase("equals(property, 45.67)", TestName = "EqualsWithDoubleValue")]
    [TestCase("equals(status, -10)", TestName = "EqualsWithNegativeInteger")]
    [TestCase("equals(score, -15.5)", TestName = "EqualsWithNegativeDouble")]
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

    [TestCase("contains(title, \"search term\")", TestName = "ContainsWithStringValue")]
    [TestCase("contains(description, 42)", TestName = "ContainsWithIntegerValue")]
    [TestCase("contains(content, 3.14)", TestName = "ContainsWithDoubleValue")]
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

    [TestCase("EQUALS(name, \"value\")", TestName = "UppercaseEquals")]
    [TestCase("equals(name, \"value\")", TestName = "LowercaseEquals")]
    [TestCase("Equals(name, \"value\")", TestName = "MixedCaseEquals")]
    [TestCase("CONTAINS(title, \"search\")", TestName = "UppercaseContains")]
    [TestCase("contains(title, \"search\")", TestName = "LowercaseContains")]
    [TestCase("Contains(title, \"search\")", TestName = "MixedCaseContains")]
    [TestCase("AND(equals(name, \"test\"), contains(title, \"title\"))", TestName = "UppercaseAnd")]
    [TestCase("and(equals(name, \"test\"), contains(title, \"title\"))", TestName = "LowercaseAnd")]
    [TestCase("And(equals(name, \"test\"), contains(title, \"title\"))", TestName = "MixedCaseAnd")]
    [TestCase("OR(equals(name, \"test\"), contains(title, \"title\"))", TestName = "UppercaseOr")]
    [TestCase("or(equals(name, \"test\"), contains(title, \"title\"))", TestName = "LowercaseOr")]
    [TestCase("Or(equals(name, \"test\"), contains(title, \"title\"))", TestName = "MixedCaseOr")]
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

    [TestCase("and(equals(name, \"test\"), contains(title, \"search\"))", TestName = "AndWithTwoFilters")]
    [TestCase("and(equals(status, \"active\"), contains(description, \"keyword\"), equals(type, 1))", TestName = "AndWithThreeFilters")]
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

    [TestCase("or(equals(name, \"test\"), contains(title, \"search\"))", TestName = "OrWithTwoFilters")]
    [TestCase("or(equals(status, \"active\"), contains(description, \"keyword\"), equals(type, 1))", TestName = "OrWithThreeFilters")]
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

    [TestCase("invalid syntax", TestName = "InvalidSyntax")]
    [TestCase("equals()", TestName = "MissingParameters")]
    [TestCase("equals(name)", TestName = "MissingValue")]
    [TestCase("equals(name, \"value\", extra)", TestName = "TooManyParameters")]
    [TestCase("and()", TestName = "EmptyAndOperation")]
    [TestCase("or(equals(name, \"test\"))", TestName = "OrWithSingleFilter")]
    [TestCase("equals(, \"value\")", TestName = "MissingFieldName")]
    [TestCase("equals(name, )", TestName = "MissingValue")]
    [TestCase("unknownop(name, \"value\")", TestName = "UnknownOperator")]
    [TestCase("equals(name \"value\")", TestName = "MissingComma")]
    [TestCase("equals[name, \"value\"]", TestName = "WrongBrackets")]
    [TestCase("equals(name, \"unclosed string)", TestName = "UnclosedString")]
    [TestCase("and(equals(name, \"test\"", TestName = "UnclosedParentheses")]
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

    [TestCase("equals(field123, \"value\")", TestName = "AlphanumericFieldName")]
    [TestCase("contains(a1b2c3, 999)", TestName = "MixedAlphanumericField")]
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

    [TestCase("contains(title, \"escaped \\\"quotes\\\" test\")", TestName = "EscapedQuotes")]
    [TestCase("equals(path, \"C:\\\\Program Files\\\\App\")", TestName = "EscapedBackslashes")]
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

    [TestCase("and(or(equals(name, \"test\"), contains(title, \"search\")), equals(status, \"active\"))", TestName = "NestedAndOr")]
    [TestCase("or(and(equals(type, 1), contains(desc, \"keyword\")), equals(flag, \"true\"))", TestName = "NestedOrAnd")]
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

    [TestCase("equals(name, 42.0)", TestName = "DoubleWithZeroDecimal")]
    [TestCase("contains(field, -0)", TestName = "NegativeZero")]
    [TestCase("equals(value, 0.123456)", TestName = "MultipleDecimals")]
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

    [Test]
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