using System.Text.RegularExpressions;

namespace Issueneter.Infrastructure.Github.Services;

internal partial class GithubInfoParser
{
    [GeneratedRegex(@"^(?<owner>[a-zA-Z0-9-_]+)\/(?<repo>[a-zA-Z0-9-_]+)$", RegexOptions.Compiled)]
    private static partial Regex GithubRepoRegex();

    [GeneratedRegex(@"[&?]page=(?<pageNumber>\d+[&a-zA-Z0-9_-]*)>; rel=""next""")]
    private static partial Regex GithubPageRegex();
    
    public static bool TryParseRepo(string target, out string owner, out string repo)
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

    public static bool TryParseNextPage(string link, out int pageNumber)
    {
        var match = GithubPageRegex().Match(link);

        if (!match.Success)
        {
            pageNumber = 0;
            return false;
        }

        pageNumber = int.Parse(match.Groups["pageNumber"].Value);
        return true;
    }
}