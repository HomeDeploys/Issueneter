using Issueneter.Common.Exceptions;
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

    public async Task<IReadOnlyCollection<GithubIssueEventEntity>> GetIssuesEvents(string owner, string repo, DateTimeOffset? since)
    {
        var isFirstRun = since is null;
        var startPage = 1;
        var domainIssues = new List<GithubIssueEventEntity>();
        
        // TODO: Rate limiter handler
        while (startPage < _configuration.PageLimitPerRun)
        {
            var uri = new Uri($"repos/{owner}/{repo}/events", UriKind.Relative);
            var eventsResponse = await _client.Connection.Get<List<ExtendedActivity>>(uri, new Dictionary<string, string>()
            {
                ["per_page"] = _configuration.PageSize.ToString(),
                ["page"] = startPage.ToString()
            });

            if (eventsResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new IssueneterException($"Github API returned status code {eventsResponse.HttpResponse.StatusCode} with body {eventsResponse.HttpResponse.Body}");
            }
            
            var events = eventsResponse.Body;
            var filteredEvents = events.Where(IsIssueActivity).Where(c => c.CreatedAt > since).Select(ToDomain);
            domainIssues.AddRange(filteredEvents);

            if (isFirstRun || events.Any(e => e.CreatedAt <= since) || events.Count() < _configuration.PageSize)
            {
                break;
            }

            startPage++;
        }

        return domainIssues.GroupBy(i => i.Id)
            .Select(g => g.Aggregate(Merge))
            .ToList();
    }

    private static bool IsIssueActivity(ExtendedActivity activity) => activity is { Type: "IssuesEvent", Payload.Action: "opened" or "labeled" };
    
    private GithubIssueEventEntity ToDomain(ExtendedActivity activity)
    {
        var issueEvent = activity.Payload;
        
        return new GithubIssueEventEntity()
        {
            Id = issueEvent.Issue.Id,
            Event = issueEvent.Action,
            Author = issueEvent.Issue.User.Login,
            Title = issueEvent.Issue.Title,
            Body = issueEvent.Issue.Body,
            Url = issueEvent.Issue.HtmlUrl,
            CreatedAt = issueEvent.Issue.CreatedAt,
            UpdatedAt = activity.CreatedAt,
            Labels = issueEvent.Action == "created" ? issueEvent.Issue.Labels.Select(l => l.Name).ToArray() : [issueEvent.Label.Name]
        };
    }

    private GithubIssueEventEntity Merge(GithubIssueEventEntity left, GithubIssueEventEntity right)
    {
        return new GithubIssueEventEntity()
        {
            Id = left.Id,
            Event = left.Event,
            Author = left.Author,
            Title = left.Title,
            Body = left.Body,
            Url = left.Url,
            CreatedAt = left.CreatedAt,
            Labels = left.Labels.Concat(right.Labels).ToHashSet(),
            UpdatedAt = left.UpdatedAt
        };
    }
}