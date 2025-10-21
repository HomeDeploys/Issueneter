using Issueneter.Infrastructure.Github.Configuration;
using Issueneter.Infrastructure.Github.Models;
using Microsoft.Extensions.Options;
using Octokit;

namespace Issueneter.Infrastructure.Github.Services;

internal class GithubClient
{
    private readonly GitHubClient _client;
    private readonly GithubClientConfiguration _configuration;

    public GithubClient(IOptions<GithubClientConfiguration> configuration)
    {
        _client = new GitHubClient(new ProductHeaderValue("Issueneter"))
        {
            Credentials = new Credentials(configuration.Value.Token)
        };
        _configuration = configuration.Value;
    }

    public async Task<bool> RepoExists(string owner, string repo)
    {
        try
        {
            await _client.Repository.Get(owner, repo);
            return true;
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<IReadOnlyCollection<GithubIssueEntity>> GetIssues(string owner, string repo, DateTimeOffset? since)
    {
        var isFirstRun = since is null;
        var startPage = 1;
        var domainIssues = new List<GithubIssueEntity>();
        
        // TODO: Rate limiter handler
        while (startPage < _configuration.PageLimitPerRun)
        {
            var events = await _client.Activity.Events.GetAllForRepository(owner, repo, new ApiOptions()
            {
                PageSize = _configuration.PageSize,
                StartPage = startPage
            });

            var filteredEvents = events.Where(IsIssueActivity).Where(c => c.CreatedAt > since).Select(ToDomain);
            domainIssues.AddRange(filteredEvents);

            if (isFirstRun || events.Any(e => e.CreatedAt <= since))
            {
                break;
            }
        }

        return domainIssues.GroupBy(i => i.Id)
            .Select(Merge)
            .ToList();
    }

    private static bool IsIssueActivity(Activity activity) => activity is { Type: "IssuesEvent", Payload: IssueEventPayload { Action: "created" or "labeled"} };
    
    private GithubIssueEntity ToDomain(Activity activity)
    {
        var issueEvent = (IssueEventPayload)activity.Payload;
        return new GithubIssueEntity()
        {
            Id = issueEvent.Issue.Id,
            Author = issueEvent.Issue.User.Login,
            Title = issueEvent.Issue.Title,
            Body = issueEvent.Issue.Body,
            CreatedAt = issueEvent.Issue.CreatedAt,
            UpdatedAt = activity.CreatedAt,
            Labels = issueEvent.Action == "labeled" 
                ? issueEvent.Issue.Labels.Select(l => l.Name).Reverse().Take(1).ToList()
                : issueEvent.Issue.Labels.Select(l => l.Name).ToList()
        };
    }

    private GithubIssueEntity Merge(IEnumerable<GithubIssueEntity> issues)
    {
        var latestIssue = issues.MaxBy(i => i.UpdatedAt)!;
        return new GithubIssueEntity()
        {
            Id = latestIssue.Id,
            Author = latestIssue.Author,
            Title = latestIssue.Title,
            Body = latestIssue.Body,
            CreatedAt = latestIssue.CreatedAt,
            UpdatedAt = latestIssue.UpdatedAt,
            Labels = issues.SelectMany(i => i.Labels).Distinct().ToList()
        }; 
    }
}