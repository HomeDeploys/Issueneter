using Issueneter.Domain.Models;

namespace Issueneter.Infrastructure.Github.Models;

internal class GithubIssueEntity : Entity
{
    public required long Id { get; set; }
    public required string Author { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required IReadOnlyList<string> Labels { get; init; }
}