using System.Text.RegularExpressions;

namespace Issueneter.Infrastructure.Github.Services;

internal partial class GithubTargetParser
{
    [GeneratedRegex(@"^(?<owner>[a-zA-Z0-9-_]+)\/(?<repo>[a-zA-Z0-9-_]+)$", RegexOptions.Compiled)]
    private static partial Regex GithubRepoRegex();
    
    public static bool TryParse(string target, out string owner, out string repo)
    {
        var match = GithubRepoRegex().Match(target.Trim());

        if (!match.Success)
        {
            owner = string.Empty;
            repo = string.Empty;
            return false;
        }

        owner = match.Groups["owner"].Value;
        repo = match.Groups["repo"].Value;
        return true;
    }
}