using FluentAssertions;
using Issueneter.Domain.Enums;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Infrastructure.Github.Configuration;
using Issueneter.Infrastructure.Github.Services;
using Issueneter.Tests.Setup;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Issueneter.Tests.Unit.Github;

public class GithubProviderTests : TestBase
{
    private readonly GithubProvider _githubProvider;

    public GithubProviderTests()
    {
        var githubClient = new GithubClient(Options.Create(new GithubClientConfiguration { Token = "token" }));
        _githubProvider = new GithubProvider(githubClient, Mock.Of<IProviderSnapshotRepo>());
    }

    [Fact]
    public void Type_Should_ReturnGithubProviderType()
    {
        _githubProvider.Type.Should().Be(ProviderType.Github, because: "GithubProvider should always return Github provider type");
    }

    [Fact]
    public void Description_Should_NotBeEmpty()
    {
        _githubProvider.Description.Should().NotBeNullOrWhiteSpace(because: "GithubProvider should describe what it does");
    }

    [Fact]
    public void TargetFormatDescription_Should_MatchValidationErrorFormat()
    {
        _githubProvider.TargetFormatDescription.Should().Be("<owner>/<repo>", because: "TargetFormatDescription should match the format described in Validate()");
    }

    [Fact]
    public void GetSample_Should_ReturnEntity_When_Called()
    {
        // Arrange
        // Act
        var sample = _githubProvider.GetSample();

        // Assert
        sample.Should().NotBeNull(because: "GetSample should always return a non-null entity");
    }
}
