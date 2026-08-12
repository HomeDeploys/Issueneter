using System.Text.RegularExpressions;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;

namespace Issueneter.Application.Commands;

internal partial class CommandParser : ICommandParser
{
    [GeneratedRegex(@"^(?<name>[a-zA-Z]+)( (?<workerId>[0-9]+))?$")]
    private static partial Regex CommandNameRegex();

    [GeneratedRegex(@"^help(?:\s+(?<type>[a-zA-Z]+))?(?:\s+(?<name>\S+))?$", RegexOptions.IgnoreCase)]
    private static partial Regex HelpCommandNameRegex();

    public ParseResult<Command> Parse(string command)
    {
        //TODO: Rewrite with spans
        if (string.IsNullOrWhiteSpace(command))
        {
            return ParseResult<Command>.Fail("Command is empty");
        }

        var lines = command.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var commandNameMatch = CommandNameRegex().Match(lines[0]);
        var workerId = WorkerId.Empty;
        var arguments = new Dictionary<string, string>();
        string commandName;

        if (commandNameMatch.Success)
        {
            commandName = commandNameMatch.Groups["name"].Value;
            if (commandNameMatch.Groups["workerId"].Success)
            {
                workerId = new WorkerId(long.Parse(commandNameMatch.Groups["workerId"].Value));
            }
        }
        else if (lines[0].StartsWith("help", StringComparison.OrdinalIgnoreCase))
        {
            // "help <type> <name>" doesn't fit the strict "<name> [<workerId>]" shape, so it needs its own regex.
            var helpCommandNameMatch = HelpCommandNameRegex().Match(lines[0]);
            if (!helpCommandNameMatch.Success)
            {
                return ParseResult<Command>.Fail($"Invalid command name: {lines[0]}");
            }

            commandName = "help";
            if (helpCommandNameMatch.Groups["type"].Success)
            {
                arguments["Type"] = helpCommandNameMatch.Groups["type"].Value;
            }
            if (helpCommandNameMatch.Groups["name"].Success)
            {
                arguments["Name"] = helpCommandNameMatch.Groups["name"].Value;
            }
        }
        else
        {
            return ParseResult<Command>.Fail($"Invalid command name: {lines[0]}");
        }

        if (commandName == "help")
        {
            // "help" is fully specified by its first line (literal "help <type> <name>" syntax,
            // matched via either regex above) - subsequent "Key: Value" lines are not part of its
            // syntax and must not be parsed as arguments.
            return ParseResult<Command>.Success(new Command(commandName, workerId, arguments));
        }

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var arg = line.Split(':', 2);
            if (arg.Length != 2)
            {
                return ParseResult<Command>.Fail($"Invalid argument format: {line} \n Expected format: \"Name: Value\"");
            }

            arguments[arg[0].Trim()] =  arg[1].Trim();
        }

        return ParseResult<Command>.Success(new Command(commandName, workerId, arguments));
    }
}