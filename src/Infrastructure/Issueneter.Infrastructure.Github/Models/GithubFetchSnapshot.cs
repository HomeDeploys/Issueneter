namespace Issueneter.Infrastructure.Github.Models;

internal class GithubFetchSnapshot
{
    public required string Owner { get; set; }
    public required string Repository { get; set; }
    public required DateTimeOffset LastIssueCreated { get; set; }
}