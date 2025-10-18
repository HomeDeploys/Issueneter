using Issueneter.Common.Exceptions;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Infrastructure.Github.Models;

namespace Issueneter.Infrastructure.Github.Services;

// TODO: Maybe move to application
internal class GithubProvider : IEntityProvider
{
    private readonly GithubClient _client;
    private readonly IProviderSnapshotRepo _snapshotRepo;

    public GithubProvider(GithubClient client, IProviderSnapshotRepo snapshotRepo)
    {
        _client = client;
        _snapshotRepo = snapshotRepo;
    }

    public ProviderType Type => ProviderType.Github;
    
    public ValidationResult Validate(string target)
    {
        // TODO: Add check if repo exists
        var parsed = GithubTargetParser.TryParse(target, out _, out _);

        if (!parsed)
        {
            return ValidationResult.Fail($"Github target must be in format <owner>/<repo>");
        }
        
        return ValidationResult.Success;
    }

    public async Task<IReadOnlyCollection<Entity>> Fetch(WorkerId workerId, string target, CancellationToken token)
    {
        var isTargetValid = GithubTargetParser.TryParse(target, out var owner, out var repo);

        if (!isTargetValid)
        {
            throw new InvalidTargetException($"Invalid github target: {target}");
        }

        var snapshot = await _snapshotRepo.GetLastSnapshot<GithubFetchSnapshot>(workerId, token);

        DateTimeOffset? fetchSince = null;
        if (snapshot is not null && snapshot.Owner == owner && snapshot.Repository == repo)
        {
            fetchSince = snapshot.LastIssueCreated;
        }

        var issues = await _client.GetIssues(owner, repo, fetchSince);

        var maxCreatedAt = issues.Max(i => i.CreatedAt);
        snapshot = new GithubFetchSnapshot()
        {
            Owner = owner,
            Repository = repo,
            LastIssueCreated = maxCreatedAt
        };

        // TODO: Transaction
        await _snapshotRepo.UpsertSnapshot(workerId, snapshot, token);

        return issues;
    }

    public Entity GetSample()
    {
        return new GithubIssueEntity()
        {
            Author = "Author",
            Body = "Some body",
            CreatedAt = DateTimeOffset.Now,
            Labels = ["Label"],
            Title = "Title"
        };
    }
}