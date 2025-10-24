using Issueneter.Application.Parser;
using Issueneter.Application.Parser.Unary;
using NUnit.Framework;

namespace Issueneter.Tests.Unit;

[TestFixture]
public class FilterParserTests
{
    private readonly FilterParser _filterParser = new FilterParser();

    [Test]
    public void Parse_EmptyString_ReturnsEmptyFilter()
    {
        // Act
        var result = _filterParser.Parse("");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Entity, Is.InstanceOf<EmptyFilter>());
    }

    [Test]
    public void Parse_WhitespaceString_ReturnsEmptyFilter()
    {
        // Act
        var result = _filterParser.Parse("   \t\n\r  ");

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Entity, Is.InstanceOf<EmptyFilter>());
    }

    [TestCase("equals(name, \"value\")", TestName = "EqualsWithStringValue")]
    [TestCase("equals(field, 123)", TestName = "EqualsWithIntegerValue")]
    [TestCase("equals(property, 45.67)", TestName = "EqualsWithDoubleValue")]
    [TestCase("equals(status, -10)", TestName = "EqualsWithNegativeInteger")]
    [TestCase("equals(score, -15.5)", TestName = "EqualsWithNegativeDouble")]
    public void Parse_SimpleEqualsOperation_Succeeds(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Filter should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
        Assert.That(result.Entity, Is.Not.InstanceOf<EmptyFilter>());
    }

    [TestCase("contains(title, \"search term\")", TestName = "ContainsWithStringValue")]
    [TestCase("contains(description, 42)", TestName = "ContainsWithIntegerValue")]
    [TestCase("contains(content, 3.14)", TestName = "ContainsWithDoubleValue")]
    public void Parse_SimpleContainsOperation_Succeeds(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Filter should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
        Assert.That(result.Entity, Is.Not.InstanceOf<EmptyFilter>());
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
    public void Parse_CaseInsensitiveOperators_Work(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Filter should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
        Assert.That(result.Entity, Is.Not.InstanceOf<EmptyFilter>());
    }

    [TestCase("and(equals(name, \"test\"), contains(title, \"search\"))", TestName = "AndWithTwoFilters")]
    [TestCase("and(equals(status, \"active\"), contains(description, \"keyword\"), equals(type, 1))", TestName = "AndWithThreeFilters")]
    public void Parse_AndOperation_WithFilters(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Filter should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
        Assert.That(result.Entity, Is.Not.InstanceOf<EmptyFilter>());
    }

    [TestCase("or(equals(name, \"test\"), contains(title, \"search\"))", TestName = "OrWithTwoFilters")]
    [TestCase("or(equals(status, \"active\"), contains(description, \"keyword\"), equals(type, 1))", TestName = "OrWithThreeFilters")]
    public void Parse_OrOperation_WithFilters(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Filter should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
        Assert.That(result.Entity, Is.Not.InstanceOf<EmptyFilter>());
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
    public void Parse_InvalidSyntax_ReturnsFailure(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.False, "Invalid syntax should return failure");
        Assert.That(result.Entity, Is.Null);
        Assert.That(result.Error, Is.Not.Empty, "Error message should be provided for invalid syntax");
    }

    [TestCase("equals(field123, \"value\")", TestName = "AlphanumericFieldName")]
    [TestCase("contains(a1b2c3, 999)", TestName = "MixedAlphanumericField")]
    public void Parse_ValidFieldNames_Succeed(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Valid field names should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
    }

    [TestCase("contains(title, \"escaped \\\"quotes\\\" test\")", TestName = "EscapedQuotes")]
    [TestCase("equals(path, \"C:\\\\Program Files\\\\App\")", TestName = "EscapedBackslashes")]
    public void Parse_EscapedStrings_Succeed(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Escaped strings should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
    }

    [TestCase("and(or(equals(name, \"test\"), contains(title, \"search\")), equals(status, \"active\"))", TestName = "NestedAndOr")]
    [TestCase("or(and(equals(type, 1), contains(desc, \"keyword\")), equals(flag, \"true\"))", TestName = "NestedOrAnd")]
    public void Parse_NestedOperations_Succeed(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Nested operations should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
        Assert.That(result.Entity, Is.Not.InstanceOf<EmptyFilter>());
    }

    [TestCase("equals(name, 42.0)", TestName = "DoubleWithZeroDecimal")]
    [TestCase("contains(field, -0)", TestName = "NegativeZero")]
    [TestCase("equals(value, 0.123456)", TestName = "MultipleDecimals")]
    public void Parse_VariousNumberFormats_Succeed(string filter)
    {
        // Act
        var result = _filterParser.Parse(filter);

        // Assert
        Assert.That(result.IsSuccess, Is.True, $"Various number formats should parse successfully. Error: {result.Error}");
        Assert.That(result.Entity, Is.Not.Null);
    }

    [Test]
    public void Parse_NullInput_ReturnsEmptyFilter()
    {
        // Act
        var result = _filterParser.Parse(null!);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Entity, Is.InstanceOf<EmptyFilter>());
    }
}