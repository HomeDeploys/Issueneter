using Issueneter.Domain.Attributes;
using Issueneter.Domain.Models;

namespace Issueneter.Infrastructure.Github.Models;

internal class GithubIssueEventEntity : Entity
{
    [FieldDescription("GitHub issue ID.")]
    public required long Id { get; set; }

    [FieldDescription("Event type, e.g. \"opened\" or \"labeled\".")]
    public required string Event { get; set; }

    [FieldDescription("GitHub login of the issue author.")]
    public required string Author { get; init; }

    [FieldDescription("Issue title.")]
    public required string Title { get; init; }

    [FieldDescription("Issue body/description.")]
    public required string Body { get; init; }

    [FieldDescription("URL of the issue.")]
    public required string Url { get; init; }

    [FieldDescription("Date and time the issue was created.")]
    public required DateTimeOffset CreatedAt { get; init; }

    [FieldDescription("Date and time this event occurred.")]
    public required DateTimeOffset UpdatedAt { get; init; }

    [FieldDescription("Labels currently on the issue.")]
    public required IReadOnlyCollection<string> Labels { get; init; }

    [FieldDescription("Labels changed (added) by this event.")]
    public required IReadOnlyCollection<string> ChangedLabels { get; init; }
}