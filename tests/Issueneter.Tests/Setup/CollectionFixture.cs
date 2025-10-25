using Xunit;

namespace Issueneter.Tests.Setup;

[CollectionDefinition(CollectionName)]
public class CollectionFixture : ICollectionFixture<TestFixture>
{
    public const string CollectionName = "Issueneter.Tests";
}