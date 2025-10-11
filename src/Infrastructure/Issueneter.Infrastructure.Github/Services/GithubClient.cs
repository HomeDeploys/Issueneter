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
            var issues = await _client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest()
            {
                Since = since,
                SortDirection = SortDirection.Descending,
                SortProperty = IssueSort.Created,
            }, new ApiOptions()
            {
                PageSize = _configuration.PageSize,
                StartPage = startPage
            });

            domainIssues.AddRange(issues.Select(ToDomain));

            if (isFirstRun || issues.Count < _configuration.PageSize)
            {
                break;
            }
        }

        return domainIssues;
    }

    private GithubIssueEntity ToDomain(Issue issue)
    {
        return new GithubIssueEntity()
        {
            Author = issue.User.Login,
            Title = issue.Title,
            Body = issue.Body,
            CreatedAt = issue.CreatedAt,
            Labels = issue.Labels.Select(l => l.Name).ToList(),
        };
    }
}