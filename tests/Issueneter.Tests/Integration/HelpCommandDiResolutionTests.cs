using FluentAssertions;
using Issueneter.Application;
using Issueneter.Application.Commands.Handlers;
using Issueneter.Application.Parser;
using Issueneter.Domain.Interfaces.Commands;
using Issueneter.Domain.Interfaces.Connection;
using Issueneter.Domain.Interfaces.Repos;
using Issueneter.Domain.Interfaces.Services;
using Issueneter.Domain.Models;
using Issueneter.Domain.ValueObjects;
using Issueneter.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Issueneter.Tests.Integration;

/// <summary>
/// Exercises the real (unmocked) DI container built from
/// <see cref="ServiceCollectionExtensions.AddApplicationServices"/> to prove that
/// <see cref="HelpCommandHandler"/> - which resolves <see cref="IEnumerable{ICommandHandler}"/>
/// lazily via <see cref="IServiceProvider"/> instead of constructor injection - does not
/// introduce a circular dependency when the container actually builds the ICommandHandler graph.
/// </summary>
public class HelpCommandDiResolutionTests : TestBase
{
    [Fact]
    public void Get_Should_ResolveHelpCommandHandler_When_ContainerIsBuiltFromAddApplicationServices()
    {
        // Arrange: build the real DI container the same way the composition root does
        // (ServiceCollectionExtensions.AddApplicationServices), plus the filter parser
        // (registered by the sibling Application.Filters project) and minimal stand-ins for
        // the Domain.Interfaces dependencies of the other ICommandHandlers (Create/Update/Get/
        // Delete) so the whole ICommandHandler graph can actually be constructed. No real
        // Infrastructure (Database/Github/Telegram) is registered, and no IEntityProvider/IClient
        // registrations are needed - this test targets the ICommandHandler graph only.
        var services = new ServiceCollection();

        services.AddApplicationServices();
        services.AddFilters();

        services.AddScoped(_ => Mock.Of<IWorkerConfigurationRepo>());
        services.AddScoped(_ => Mock.Of<IScheduler>());
        services.AddScoped(_ => Mock.Of<ITransactionProvider>());

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<ICommandHandlerFactory>();
        var command = new Command("help", WorkerId.Empty, new Dictionary<string, string>());

        // Act
        var act = () => factory.Get(command);

        // Assert
        act.Should().NotThrow(because: "resolving the help command must not trigger a circular DI dependency");
        var handler = act();
        handler.Should().BeOfType<HelpCommandHandler>(because: "the 'help' command should be routed to HelpCommandHandler");
    }
}
