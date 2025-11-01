using Octokit;

namespace Issueneter.Infrastructure.Github.Models;

internal class ExtendedActivity
{
    public required string Type { get; set; }
    public required ExtendedIssueEventPayload  Payload { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}

internal class ExtendedIssueEventPayload
{
    public required string Action { get; set; }
    public required Issue Issue { get; set; }
    public required Label Label { get; set; }
} 