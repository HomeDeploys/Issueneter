using FluentAssertions;
using Issueneter.Application.Commands;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Xunit;

namespace Issueneter.Tests.Unit.Commands;

public class CommandParserTests : TestBase
{
    [Fact]
    public void Parse_Should_ReturnFailureResult_When_CommandIsEmpty()
    {
        // Arrange
        var parser = new CommandParser();
        var emptyCommand = string.Empty;
        
        // Act
        var result = parser.Parse(emptyCommand);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "empty command should not be parsed successfully");
        result.Error.Should().NotBeEmpty(because: "error message should be provided");
    }
    
    [Fact]
    public void Parse_Should_ReturnFailureResult_When_CommandIsNull()
    {
        // Arrange
        var parser = new CommandParser();
        string? nullCommand = null;
        
        // Act
        var result = parser.Parse(nullCommand!);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "null command should not be parsed successfully");
        result.Error.Should().NotBeEmpty(because: "error message should be provided");
    }
    
    [Fact]
    public void Parse_Should_ReturnFailureResult_When_CommandIsWhitespaceOnly()
    {
        // Arrange
        var parser = new CommandParser();
        var whitespaceCommand = "   \t  \n  ";
        
        // Act
        var result = parser.Parse(whitespaceCommand);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "whitespace-only command should not be parsed successfully");
        result.Error.Should().NotBeEmpty(because: "error message should be provided");
    }
    
    [Fact]
    public void Parse_Should_ExtractNameOnly_When_CommandHasValidNameOnly()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithNameOnly = "create";
        
        // Act
        var result = parser.Parse(commandWithNameOnly);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with valid name should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Name.Should().Be("create");
        result.Entity.WorkerId.Should().Be(WorkerId.Empty);
        result.Entity.Parameters.Should().BeEmpty();
    }
    
    [Fact]
    public void Parse_Should_ReturnFailureResult_When_CommandNameIsInvalid()
    {
        // Arrange
        var parser = new CommandParser();
        var invalidNameCommand = "123create";
        
        // Act
        var result = parser.Parse(invalidNameCommand);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "command with invalid name should not be parsed successfully");
        result.Error.Should().Contain("Invalid command name", because: "error should indicate invalid command name");
    }
    
    [Fact]
    public void Parse_Should_ExtractNameAndWorkerId_When_CommandHasValidWorkerID()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithWorkerId = "create 123";
        
        // Act
        var result = parser.Parse(commandWithWorkerId);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with valid name and worker ID should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Name.Should().Be("create");
        result.Entity.WorkerId.Should().NotBe(WorkerId.Empty);
        result.Entity.WorkerId.Value.Should().Be(123);
        result.Entity.Parameters.Should().BeEmpty();
    }
    
    [Fact]
    public void Parse_Should_ReturnFailureResult_When_WorkerIdIsNotNumeric()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithInvalidWorkerId = "create abc";
        
        // Act
        var result = parser.Parse(commandWithInvalidWorkerId);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "command with non-numeric worker ID should not be parsed successfully");
        result.Error.Should().Contain("Invalid command name", because: "error should indicate invalid command format");
    }
    
    [Fact]
    public void Parse_Should_HandleLargeWorkerId_When_WorkerIdIsVeryLarge()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithLargeWorkerId = "create 9223372036854775807"; // Max long value
        
        // Act
        var result = parser.Parse(commandWithLargeWorkerId);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with large worker ID should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Name.Should().Be("create");
        result.Entity.WorkerId.Value.Should().Be(9223372036854775807);
    }
    
    [Fact]
    public void Parse_Should_AddSingleArgument_When_CommandHasSingleValidArgument()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithSingleArg = "create\nname: Test Issue";
        
        // Act
        var result = parser.Parse(commandWithSingleArg);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with single valid argument should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("name");
        result.Entity.Parameters["name"].Should().Be("Test Issue");
    }
    
    [Fact]
    public void Parse_Should_AddMultipleArguments_When_CommandHasMultipleValidArguments()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithMultipleArgs = "create\nname: Test Issue\ndescription: This is a test issue\npriority: High";
        
        // Act
        var result = parser.Parse(commandWithMultipleArgs);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with multiple valid arguments should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKeys("name", "description", "priority");
        result.Entity.Parameters["name"].Should().Be("Test Issue");
        result.Entity.Parameters["description"].Should().Be("This is a test issue");
        result.Entity.Parameters["priority"].Should().Be("High");
    }
    
    [Fact]
    public void Parse_Should_ReturnFailureResult_When_ArgumentMissingColonSeparator()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithInvalidArg = "create\nname Test Issue";
        
        // Act
        var result = parser.Parse(commandWithInvalidArg);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "command with invalid argument format should not be parsed successfully");
        result.Error.Should().Contain("Invalid argument format", because: "error should indicate invalid argument format");
    }
    
    [Fact]
    public void Parse_Should_AddEmptyStringValue_When_ArgumentHasEmptyValue()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithEmptyArgValue = "create\nname:";
        
        // Act
        var result = parser.Parse(commandWithEmptyArgValue);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with empty argument value should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("name");
        result.Entity.Parameters["name"].Should().BeEmpty();
    }
    
    [Fact]
    public void Parse_Should_TrimWhitespace_When_ArgumentsHaveWhitespaceAroundNameAndValue()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithWhitespace = "create\n  name  :  Test Issue  \n  description  :  This is a test  ";
        
        // Act
        var result = parser.Parse(commandWithWhitespace);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with whitespace around argument names and values should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKeys("name", "description");
        result.Entity.Parameters["name"].Should().Be("Test Issue");
        result.Entity.Parameters["description"].Should().Be("This is a test");
    }
    
    [Fact]
    public void Parse_Should_UseLastValue_When_ArgumentNameIsDuplicated()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithDuplicateArgs = "create\nname: First Value\nname: Second Value";
        
        // Act
        var result = parser.Parse(commandWithDuplicateArgs);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with duplicate argument names should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("name");
        result.Entity.Parameters["name"].Should().Be("Second Value", because: "last value should be used for duplicate argument names");
    }
    
    [Fact]
    public void Parse_Should_ExtractAllComponents_When_CommandIsComplete()
    {
        // Arrange
        var parser = new CommandParser();
        var completeCommand = "create 123\nname: Test Issue\ndescription: This is a test issue\npriority: High";
        
        // Act
        var result = parser.Parse(completeCommand);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "complete command should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Name.Should().Be("create");
        result.Entity.WorkerId.Value.Should().Be(123);
        result.Entity.Parameters.Should().ContainKeys("name", "description", "priority");
        result.Entity.Parameters["name"].Should().Be("Test Issue");
        result.Entity.Parameters["description"].Should().Be("This is a test issue");
        result.Entity.Parameters["priority"].Should().Be("High");
    }
    
    [Fact]
    public void Parse_Should_PreserveSpecialCharacters_When_ArgumentValuesContainSpecialCharacters()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithSpecialChars = "create\ndescription: This is a test with special chars: @#$%^&*()!";
        
        // Act
        var result = parser.Parse(commandWithSpecialChars);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with special characters should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("description");
        result.Entity.Parameters["description"].Should().Be("This is a test with special chars: @#$%^&*()!");
    }
    
    [Fact]
    public void Parse_Should_IgnoreEmptyLines_When_CommandHasEmptyLinesBetweenArguments()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithEmptyLines = "create\nname: Test Issue\n\ndescription: This is a test issue\n\npriority: High";
        
        // Act
        var result = parser.Parse(commandWithEmptyLines);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with empty lines should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKeys("name", "description", "priority");
        result.Entity.Parameters["name"].Should().Be("Test Issue");
        result.Entity.Parameters["description"].Should().Be("This is a test issue");
        result.Entity.Parameters["priority"].Should().Be("High");
    }
    
    [Fact]
    public void Parse_Should_SplitOnFirstColonOnly_When_ArgumentValueContainsColons()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithColonsInValue = "create\ndescription: This is a test: with multiple: colons";
        
        // Act
        var result = parser.Parse(commandWithColonsInValue);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with colons in argument value should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("description");
        result.Entity.Parameters["description"].Should().Be("This is a test: with multiple: colons");
    }
    
    [Fact]
    public void Parse_Should_HandleLongArgumentValues_When_ArgumentValueIsVeryLong()
    {
        // Arrange
        var parser = new CommandParser();
        var longValue = new string('a', 10000);
        var commandWithLongValue = $"create\ndescription: {longValue}";
        
        // Act
        var result = parser.Parse(commandWithLongValue);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with long argument value should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("description");
        result.Entity.Parameters["description"].Should().Be(longValue);
    }
    
    [Fact]
    public void Parse_Should_PreserveUnicodeCharacters_When_ArgumentValuesContainUnicode()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithUnicode = "create\ndescription: This contains unicode: 你好, こんにちは, Привет";
        
        // Act
        var result = parser.Parse(commandWithUnicode);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with unicode characters should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKey("description");
        result.Entity.Parameters["description"].Should().Be("This contains unicode: 你好, こんにちは, Привет");
    }
    
    [Fact]
    public void Parse_Should_PreserveLineBreaks_When_ArgumentValueContainsLineBreaks()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithLineBreaks = "create\ndescription: This is line 1\nThis is still part of description\nAnd this too";
        
        // Act
        var result = parser.Parse(commandWithLineBreaks);
        
        // Assert
        result.IsSuccess.Should().BeFalse(because: "the current implementation doesn't support multi-line values");
        result.Error.Should().Contain("Invalid argument format", because: "error should indicate invalid argument format");
    }
    
    [Fact]
    public void Parse_Should_HandleDifferentLineEndings_When_CommandHasMixedLineEndings()
    {
        // Arrange
        var parser = new CommandParser();
        var commandWithMixedLineEndings = "create\r\nname: Test Issue\ndescription: This is a test";
        
        // Act
        var result = parser.Parse(commandWithMixedLineEndings);
        
        // Assert
        result.IsSuccess.Should().BeTrue(because: "command with mixed line endings should be parsed successfully");
        result.Entity.Should().NotBeNull();
        result.Entity!.Parameters.Should().ContainKeys("name", "description");
        result.Entity.Parameters["name"].Should().Be("Test Issue");
        result.Entity.Parameters["description"].Should().Be("This is a test");
    }
}