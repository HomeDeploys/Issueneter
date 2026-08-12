# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Issueneter is a .NET 10 service that polls external "providers" (currently GitHub) for
activity, filters it with a small user-authored query language, formats matching items
into messages, and sends them to "clients" (currently Telegram). Each poll/filter/format/send
pipeline is a **worker**, configured per-user via Telegram bot commands and run on a cron
schedule via Hangfire.

## Commands

```bash
dotnet restore Issueneter.sln
dotnet build Issueneter.sln --configuration Release
dotnet test Issueneter.sln --configuration Release

# Run a single test class or method (xunit.v3 via VSTest filter)
dotnet test --filter "FullyQualifiedName~EqualityFilterTests"
dotnet test --filter "FullyQualifiedName~EqualityFilterTests.IsValid_Should_ReturnTrue_When_PropertyIsCastableToGenericType"

# Local Postgres for manual runs
docker-compose up -d postgres
```

The app needs `src/Issueneter/appsettings.yaml` (copy from `appsettings.Example.yaml`) with a
GitHub token, Telegram bot token, and Postgres connection string. `CONFIG_PATH` env var, if set,
points to a directory holding an environment-specific `appsettings.{Environment}.yaml` overlay.
CI (`.github/workflows/workflow.yml`) builds/tests on every push and, on `master`, tags a release
and publishes a Docker image (`Dockerfile` is a two-stage SDK build → aspnet runtime image).

## Architecture

Clean Architecture, enforced by project references (`TreatWarningsAsErrors` is on everywhere;
nullable + implicit usings enabled repo-wide via `Directory.Build.props`). Package versions are
centrally managed in `Directory.Packages.props`.

```
src/Issueneter                       ASP.NET Core host: composition root, Program.cs,
                                      Hangfire dashboard, appsettings.

src/Domain/Issueneter.Domain             Records/enums/value objects (WorkerConfiguration,
                                          Entity, WorkerId, ParseResult, ValidationResult...).
src/Domain/Issueneter.Domain.Interfaces  All cross-layer contracts (ICommandHandler,
                                          IEntityProvider, IClient, IFilter, IScheduler,
                                          repo interfaces). Nothing but interfaces/no impl.

src/Application/Issueneter.Application         Use cases: Worker (the pipeline above),
                                                Telegram command parsing/handling
                                                (Commands/), factories that pick a provider/
                                                client/handler by enum type.
src/Application/Issueneter.Application.Filters ANTLR4-generated parser for the worker filter
                                                DSL (Query.g4 → Codegen/), plus the Filter/
                                                FilterBuilder that walks the parse tree into
                                                IFilter trees (And/Or/Not/Equals/Contains).

src/Infrastructure/Issueneter.Infrastructure.Database    Dapper repos over Postgres,
                                                           FluentMigrator migrations
                                                           (Migrations/NNN_*.cs).
src/Infrastructure/Issueneter.Infrastructure.Background   Hangfire-backed IScheduler; cron
                                                           validated via Cronos.
src/Infrastructure/Issueneter.Infrastructure.Github        Octokit-based IEntityProvider.
src/Infrastructure/Issueneter.Infrastructure.Telegram       Telegram.Bot-based IClient +
                                                              the bot message handler.
src/Issueneter.Common                Shared exceptions/extensions used by every layer.

tests/Issueneter.Tests               Single xunit.v3 test project covering filters,
                                      command handlers, and services (unit-level; DB/network
                                      dependencies are mocked with Moq).
```

Each Infrastructure/Application project registers its own services via a local
`ServiceCollectionExtensions.AddXxx(configuration)` extension method, composed together in
`src/Issueneter/Program.cs`. New providers/clients implement `IEntityProvider`/`IClient`,
register themselves in DI, and are looked up by enum (`ProviderType`/`ClientType`) through
`ProviderFactory`/`ClientFactory` — no other wiring is needed for the Worker to pick them up.

### Worker pipeline (`Issueneter.Application/Services/Worker.cs`)

1. Load `WorkerConfiguration` by `WorkerId` from `IWorkerConfigurationRepo`.
2. Resolve `IEntityProvider` (by `ProviderInfo.Type`) and `IClient` (by `ClientInfo.Type`).
3. `provider.Fetch(...)` — each provider tracks its own incremental-fetch cursor via
   `IProviderSnapshotRepo` (e.g. GitHub stores last-updated timestamp per worker+repo).
4. Parse `WorkerConfiguration.Filter` (the DSL below) via `IFilterParser`, apply it to fetched
   entities.
5. Format each matching entity with `IMessageFormatter` using `WorkerConfiguration.Template`.
6. Send each message via the resolved `IClient`.
7. Scheduling of step 1–6 as a recurring job is a separate concern (`IScheduler` /
   Hangfire `IRecurringJobManager`), driven by cron expressions from `Command` handlers.

### Filter DSL (ANTLR grammar: `Query.g4`)

`AND(...)`, `OR(...)`, `NOT(expr)`, `EQUALS(field, value)`, `CONTAINS(field, value)` — operator
keywords are case-insensitive. `FilterBuilder` (Parser/) turns the ANTLR parse tree into
`IFilter` implementations under `Binary/` and `Unary/`, which apply to an `Entity` via
reflection over its properties by name.

### Telegram command protocol

Users message the bot in Telegram; `TelegramHandler.OnMessage` checks the message is a private
chat from an allow-listed user ID (`TelegramClientConfiguration.AllowedUsers`), then
`CommandParser` parses first-line `"<name> [<workerId>]"` + subsequent `"Key: Value"` lines into
a `Command`, which `CommandHandlerFactory` routes to the matching `ICommandHandler`
(Create/Update/Get/Delete, under `Commands/Handlers/`).

## Code style

These conventions apply across the codebase (previously tracked in `AiCodestyle.md`, now folded
in here):

- Repository pattern: interfaces in `Domain.Interfaces/Repos`, Dapper implementations in
  `Infrastructure.Database/Repos`. Table/column names are snake_case; table names are `const
  string TableName` in the repo class; SQL is raw string literals with parameters bound via
  `@{nameof(dto.Property)}`, columns listed explicitly (no `SELECT *` for writes). Dispose
  connections with `await using`.
- Result pattern (`ParseResult<T>`, `ValidationResult`) for expected failures; exceptions
  (`Issueneter.Common.Exceptions`) for the unexpected. Don't throw for validation failures that
  a caller is expected to handle.
- Tests: xunit.v3 + FluentAssertions + Moq + Bogus (`Faker`), one test class per class under
  test, mirroring `Unit/<Area>/<ClassName>Tests.cs`. Name tests
  `Action_Should_Outcome[_When_Precondition]`, structure bodies with `// Arrange` / `// Act` /
  `// Assert` comments, and pass a `because:`/message argument to FluentAssertions calls.
  Parametrize same-behavior-different-input cases with `[Theory]`/`[InlineData(..., TestDisplayName = "...")]`.