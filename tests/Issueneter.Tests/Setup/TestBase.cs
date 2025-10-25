using Bogus;
using Xunit;

namespace Issueneter.Tests.Setup;

[Collection(CollectionFixture.CollectionName)]
public class TestBase
{
    protected static readonly Faker Faker = new Faker();
}