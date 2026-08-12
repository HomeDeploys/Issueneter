using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Services;

namespace Issueneter.Application.Commands;

internal static class HelpFormatter
{
    public static string FormatCommandList(IEnumerable<ICommandHandler> handlers)
    {
        var lines = handlers
            .OrderBy(handler => handler.Name, StringComparer.OrdinalIgnoreCase)
            .Select(handler => $"- {handler.Name}: {handler.Description}");

        return $"""
                Available commands:
                {string.Join(Environment.NewLine, lines)}

                Use "help command <name>" for details on a specific command.
                """;
    }

    public static string FormatCommandDetail(ICommandHandler handler)
    {
        var argumentsSection = handler.Arguments.Count == 0
            ? "Arguments: none"
            : "Arguments:" + Environment.NewLine + string.Join(
                Environment.NewLine,
                handler.Arguments.Select(argument =>
                    $"- {argument.Name} ({(argument.IsRequired ? "required" : "optional")}): {argument.Description}"));

        return $"""
                {handler.Name}
                {handler.Description}

                {argumentsSection}

                Example:
                {handler.Example}
                """;
    }

    public static string FormatProviderList(IEnumerable<IEntityProvider> providers)
    {
        var lines = providers
            .OrderBy(provider => provider.Type.ToString(), StringComparer.OrdinalIgnoreCase)
            .Select(provider => $"- {provider.Type}: {provider.Description}");

        return $"""
                Available providers:
                {string.Join(Environment.NewLine, lines)}

                Use "help provider <name>" for details on a specific provider.
                """;
    }

    public static string FormatProviderDetail(IEntityProvider provider)
    {
        var fieldLines = provider.GetSample().GetFields()
            .Select(field =>
                $"- {field.Name} ({FriendlyTypeName(field.PropertyType)})"
                + (field.Description is null ? string.Empty : $": {field.Description}"));

        return $"""
                {provider.Type}
                {provider.Description}

                Target format: {provider.TargetFormatDescription}

                Fields:
                {string.Join(Environment.NewLine, fieldLines)}
                """;
    }

    public static string FormatClientList(IEnumerable<IClient> clients)
    {
        var lines = clients
            .OrderBy(client => client.Type.ToString(), StringComparer.OrdinalIgnoreCase)
            .Select(client => $"- {client.Type}: {client.Description}");

        return $"""
                Available clients:
                {string.Join(Environment.NewLine, lines)}

                Use "help client <name>" for details on a specific client.
                """;
    }

    public static string FormatClientDetail(IClient client)
    {
        return $"""
                {client.Type}
                {client.Description}

                Target format: {client.TargetFormatDescription}
                """;
    }

    private static string FriendlyTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var backtickIndex = type.Name.IndexOf('`');
        var genericTypeName = backtickIndex > 0 ? type.Name[..backtickIndex] : type.Name;
        var genericArguments = string.Join(", ", type.GetGenericArguments().Select(FriendlyTypeName));

        return $"{genericTypeName}<{genericArguments}>";
    }
}
