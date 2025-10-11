namespace Issueneter.Infrastructure.Github.Configuration;

public class GithubClientConfiguration
{
    public required string Token { get; set; }
    public int PageSize { get; set; } = 100;
    public int PageLimitPerRun { get; set; } = 25;
    public int FirstRunFetchLimit { get; set; } = 50;
}