# Прогресс: Telegram `help` команда

Резюме каждого принятого шага. Черновик пишет `step-implementer` сразу
после реализации в `.pending/<id>.md`; координатор переносит его сюда
через `finalize_step.py` (устанавливается глобально) в момент, когда
пользователь явно одобряет шаг — вручную этот файл обычно редактировать
не нужно.

---

<!--
Формат одной записи (добавляется finalize_step.py автоматически):

## Шаг <id>: <название> (done, одобрено пользователем <дата>, коммит — за пользователем)

Что изменено: ...
Новые/изменённые контракты: ...
Ограничения и edge cases: ...
Затронутые файлы: ...
-->

## Шаг 1 (done, одобрено пользователем 2026-08-12 10:13 UTC, коммит — за пользователем)

## Резюме шага 1

Что изменено: добавлены новые Domain-модели/атрибут для будущей `help`
команды: `CommandArgumentInfo`, `FieldDescriptionAttribute`,
`EntityFieldInfo`, а также метод `Entity.GetFields()`, читающий public
instance свойства через рефлексию и опциональный
`FieldDescriptionAttribute`. Никакой существующий код эти типы пока не
использует.

Новые/изменённые контракты:
- `public record CommandArgumentInfo(string Name, string Description, bool IsRequired)` —
  `src/Domain/Issueneter.Domain/Models/CommandArgumentInfo.cs`.
- `public record EntityFieldInfo(string Name, Type PropertyType, string? Description)` —
  `src/Domain/Issueneter.Domain/Models/EntityFieldInfo.cs`.
- `[AttributeUsage(AttributeTargets.Property)] public sealed class FieldDescriptionAttribute(string description) : Attribute` —
  `src/Domain/Issueneter.Domain/Attributes/FieldDescriptionAttribute.cs`.
- `public IReadOnlyCollection<EntityFieldInfo> Entity.GetFields()` — новый
  публичный метод на `Entity` (`src/Domain/Issueneter.Domain/Models/Entity.cs`),
  использует `BindingFlags.Public | BindingFlags.Instance` (та же
  видимость, что `Entity.HasProperty`), не `NonPublic` как
  `Entity.GetProperty`.

Ограничения и edge cases: `GetFields()` не разворачивает `Nullable<T>` до
underlying-типа и не различает nullable/non-nullable reference-типы —
`PropertyType` это ровно `PropertyInfo.PropertyType` как есть; это
соответствует формулировке DoD шага (просто рефлексия + атрибут) и не
было запрошено иначе. Порядок полей в возвращаемой коллекции — порядок
`Type.GetProperties()` (не гарантированный контрактом .NET, но
детерминированный на практике); тесты сравнивают набор через
`BeEquivalentTo` без учёта порядка, а не `Should().Equal(...)`.

Затронутые файлы:
- `src/Domain/Issueneter.Domain/Models/CommandArgumentInfo.cs` (новый)
- `src/Domain/Issueneter.Domain/Models/EntityFieldInfo.cs` (новый)
- `src/Domain/Issueneter.Domain/Attributes/FieldDescriptionAttribute.cs` (новый)
- `src/Domain/Issueneter.Domain/Models/Entity.cs` (добавлен `using
  Issueneter.Domain.Attributes;` и метод `GetFields()`)
- `tests/Issueneter.Tests/Unit/Models/EntityTests.cs` (новый, использует
  приватную тестовую фикстуру `EntityFieldsFixtureEntity`, отдельную от
  общего `Issueneter.Tests.Helpers.TestEntity`)

STEP_DOD_SELF_CHECK:
- `CommandArgumentInfo` record добавлен ровно с указанной сигнатурой: выполнено
- `FieldDescriptionAttribute` (Property target, sealed, конструктор `(string description)`): выполнено
- `EntityFieldInfo` record с полем `PropertyType` (не `Type`): выполнено
- `Entity.GetFields()` через `GetProperties(Public|Instance)` + `GetCustomAttribute<FieldDescriptionAttribute>()`: выполнено
- Юнит-тесты `EntityTests.cs` на отдельной фикстуре (не общий `TestEntity`), проверяют полный список полей / `Description == null` без атрибута / `Description` с текстом атрибута: выполнено
- Никакой другой код (кроме `Entity.cs`) не менялся, новые типы нигде не используются: выполнено
- `dotnet build Issueneter.sln --configuration Release` — 0 предупреждений, 0 ошибок: выполнено
- `dotnet test Issueneter.sln --configuration Release --filter "FullyQualifiedName~EntityTests"` — 4/4 пройдено: выполнено
- Полный прогон `dotnet test Issueneter.sln --configuration Release` — 231/231 пройдено, регрессий нет: выполнено

## Шаг 2 (done, одобрено пользователем 2026-08-12 10:28 UTC, коммит — за пользователем)

## Резюме шага 2

Что изменено:
- `ICommandHandler` (`src/Domain/Issueneter.Domain.Interfaces/Commands/ICommandHandler.cs`) расширен четырьмя новыми членами: `string Name { get; }`, `string Description { get; }`, `IReadOnlyCollection<CommandArgumentInfo> Arguments { get; }`, `string Example { get; }` — обычные abstract-свойства, без `required`.
- Все 4 существующих реализации обновлены, чтобы реализовать новые члены:
  - `CreateCommandHandler`: `Name = "create"`, `Arguments` — все 7 полей `CreateCommand` (`ProviderType`, `ProviderTarget`, `Schedule`, `Filter`, `ClientType`, `ClientTarget`, `Template`), все `IsRequired: true` (соответствует `CreateCommand.Parse`, который требует все поля). `Description`/`Example` описывают реальную семантику (создание воркера, пример с корректным cron/DSL-фильтром).
  - `UpdateCommandHandler`: `Name = "update"`, `Arguments` — `ProviderTarget`, `Schedule`, `Filter`, `ClientTarget`, `Template`, все `IsRequired: false`; `Description` явно упоминает, что нужен хотя бы один параметр (соответствует `UpdateCommand.Parse`).
  - `GetCommandHandler`/`DeleteCommandHandler`: `Name = "get"`/`"delete"`, `Arguments => []` (единственный вход — `WorkerId` в первой строке команды; наличие `Parameters` уже отклоняется в `Handle`).
- Существующий `CanHandle` в каждом хендлере оставлен без изменений (как явно указано в границах шага) — не переписан на использование нового `Name`.
- Добавлены проверки метаданных (`Name`/`Description`/`Arguments`/`Example`) в существующие `CreateCommandHandlerTests.cs`, `UpdateCommandHandlerTests.cs`, `GetCommandHandlerTests.cs`.
- Создан новый `tests/Issueneter.Tests/Unit/Commands/DeleteCommandHandlerTests.cs` по аналогии с `GetCommandHandlerTests.cs` (тесты `CanHandle` true/false с разным регистром, `Handle` для пустого `WorkerId`, для наличия параметров, для успешного удаления с проверкой вызовов `repo.Delete`/`scheduler.Deschedule`), плюс тест метаданных.
- **Ревизия по итогам ревью:** многострочные `Example` в `CreateCommandHandler`/`UpdateCommandHandler`, ранее собранные конкатенацией `"..." + "\n" + ...`, переписаны на raw string literal (`"""..."""`, C# 11+). Логическое содержимое и разбиение на строки не менялись — это чисто синтаксическая правка. `GetCommandHandler`/`DeleteCommandHandler.Example` — уже однострочные строки, изменений не потребовали (проверено чтением файлов).

Новые/изменённые контракты:
- `ICommandHandler` теперь требует `Name`, `Description`, `Arguments`, `Example` от любой реализации (breaking change на уровне интерфейса, компилятор ловит через CS0535 у любых пропущенных реализаций — других реализаций в решении не найдено, кроме 4 обновлённых).
- `CommandArgumentInfo` (из шага 1) используется по назначению впервые.

Ограничения и edge cases:
- Текст `Description`/`Example` — не заимствован из готового источника, сформулирован по фактической логике `Handle`/`Parse` каждого хендлера; если координатор/пользователь хочет другую формулировку — это чисто текстовое изменение, не меняющее контракт.
- `CanHandle` намеренно не переписан на `Name` (это explicit out-of-scope по инструкции шага, "optional cleanup" оставлен на усмотрение пользователя).
- Не создавались `HelpCommandHandler`/`HelpFormatter`, не менялись `IEntityProvider`/`IClient`/`GithubProvider`/`TelegramClient`/`GithubIssueEventEntity`/`CommandParser` — вне границ шага.
- Интеграционные тесты не добавлялись — по DoD шага 2 это не требуется (компилятор + юнит-тесты покрывают риск; DI-резолюция — предмет шага 6).

Затронутые файлы:
- `src/Domain/Issueneter.Domain.Interfaces/Commands/ICommandHandler.cs`
- `src/Application/Issueneter.Application/Commands/Handlers/CreateCommandHandler.cs`
- `src/Application/Issueneter.Application/Commands/Handlers/UpdateCommandHandler.cs`
- `src/Application/Issueneter.Application/Commands/Handlers/GetCommandHandler.cs`
- `src/Application/Issueneter.Application/Commands/Handlers/DeleteCommandHandler.cs`
- `tests/Issueneter.Tests/Unit/Commands/CreateCommandHandlerTests.cs`
- `tests/Issueneter.Tests/Unit/Commands/UpdateCommandHandlerTests.cs`
- `tests/Issueneter.Tests/Unit/Commands/GetCommandHandlerTests.cs`
- `tests/Issueneter.Tests/Unit/Commands/DeleteCommandHandlerTests.cs` (новый файл)

STEP_DOD_SELF_CHECK:
- `ICommandHandler` содержит новые abstract-свойства без `required`: выполнено
- `CreateCommandHandler.Arguments` — все 7 полей `CreateCommand`, все `IsRequired: true`: выполнено
- `UpdateCommandHandler.Arguments` — 5 полей `UpdateCommand`, все `IsRequired: false`, обязательность "хотя бы одно" описана в `Description`: выполнено
- `GetCommandHandler`/`DeleteCommandHandler.Arguments => []`: выполнено
- `dotnet build Issueneter.sln --configuration Release` компилируется (0 предупреждений/ошибок): выполнено
- Юнит-тесты в `CreateCommandHandlerTests`/`UpdateCommandHandlerTests`/`GetCommandHandlerTests` дополнены проверками `Name`/`Description`/`Arguments`/`Example`: выполнено
- Новый `DeleteCommandHandlerTests.cs` создан по аналогии с `GetCommandHandlerTests.cs` + проверки метаданных: выполнено
- Отдельный интеграционный сегмент не добавлялся (сознательно, по DoD шага): выполнено
- `dotnet test Issueneter.sln --configuration Release` — весь набор зелёный (246/246), без регрессий: выполнено
- (ревизия) `Example` в `CreateCommandHandler`/`UpdateCommandHandler` переписаны на raw string literal, содержимое не изменилось: выполнено
- (ревизия) `dotnet build Issueneter.sln --configuration Release` — 0 предупреждений/ошибок после правки: выполнено
- (ревизия) `dotnet test --filter "FullyQualifiedName~CreateCommandHandlerTests|FullyQualifiedName~UpdateCommandHandlerTests"` — 33/33 пройдено без изменений в тестах: выполнено

## Шаг 3 (done, одобрено пользователем 2026-08-12 11:17 UTC, коммит — за пользователем)

## Резюме шага 3

Что изменено:
- Добавлены `string Description { get; }` и `string TargetFormatDescription { get; }` в
  `IEntityProvider` и `IClient` (`Issueneter.Domain.Interfaces/Services/`).
- `GithubProvider`: реализованы `Description` ("Polls issue events (opened issues and label
  changes) from a GitHub repository.") и `TargetFormatDescription` (`"<owner>/<repo>"`, тот же
  текст, что используется в `Validate()`).
- `TelegramClient`: реализованы `Description` ("Sends a message to a Telegram chat or topic via
  the Bot API.") и `TargetFormatDescription` (`"<chatId> or <chatId>/<threadId>"`, тот же текст,
  что используется в `Validate()`).
- `GithubIssueEventEntity`: на все 10 свойств (`Id`, `Event`, `Author`, `Title`, `Body`, `Url`,
  `CreatedAt`, `UpdatedAt`, `Labels`, `ChangedLabels`) добавлены `[FieldDescription("...")]` с
  содержательными описаниями — первое реальное использование атрибута из шага 1.
- Новый тестовый файл `tests/Issueneter.Tests/Unit/Github/GithubProviderTests.cs` (папки
  `Unit/Github/` ранее не существовало) — проверяет `Type`, `Description`,
  `TargetFormatDescription`, `GetSample`.
- В `tests/Issueneter.Tests/Unit/Telegram/TelegramClientTests.cs` добавлены тесты на
  `Description` и `TargetFormatDescription`.

Новые/изменённые контракты:
- `IEntityProvider.Description : string` (get-only, абстрактное свойство интерфейса).
- `IEntityProvider.TargetFormatDescription : string` (get-only).
- `IClient.Description : string` (get-only).
- `IClient.TargetFormatDescription : string` (get-only).
Оба существующих реализатора (`GithubProvider`, `TelegramClient`) обновлены; других
реализаторов этих интерфейсов в решении нет (проверено grep'ом).

Ограничения и edge cases:
- Тексты `Description` — новая содержательная формулировка (в коде их раньше не было), не
  копирование существующего текста, в отличие от `TargetFormatDescription`, для которого DoD
  явно требовал переиспользовать существующую строку из `Validate()` — она переиспользована
  дословно для обоих классов.
- `GithubProvider`/`GithubClient` остаются `internal` — тест создаёт `GithubClient` напрямую
  через `IOptions<GithubClientConfiguration>` (не мок, т.к. класс конкретный, не интерфейс);
  видимость обеспечена существующим `InternalsVisibleTo("Issueneter.Tests")` в сборке
  Issueneter.Infrastructure.Github (без изменений — атрибут уже был из более раннего шага).
- `Entity.GetFields()` в этом шаге не используется (по плану — используется в шаге 5).
- `ICommandHandler` и обработчики команд (шаг 2), `CommandParser`/`HelpCommandHandler`/
  `HelpFormatter`/`ServiceCollectionExtensions` (шаги 4/5) не затронуты.
- Отдельный интеграционный тестовый сегмент не добавлялся — по DoD шага это не требуется
  (проверка компилятором + юнит-тестами достаточна, DI-риск вынесен в шаг 6).

Затронутые файлы:
- src/Domain/Issueneter.Domain.Interfaces/Services/IEntityProvider.cs
- src/Domain/Issueneter.Domain.Interfaces/Services/IClient.cs
- src/Infrastructure/Issueneter.Infrastructure.Github/Services/GithubProvider.cs
- src/Infrastructure/Issueneter.Infrastructure.Github/Models/GithubIssueEventEntity.cs
- src/Infrastructure/Issueneter.Infrastructure.Telegram/Services/TelegramClient.cs
- tests/Issueneter.Tests/Unit/Telegram/TelegramClientTests.cs
- tests/Issueneter.Tests/Unit/Github/GithubProviderTests.cs (новый файл)

STEP_DOD_SELF_CHECK:
- `IEntityProvider`/`IClient` получили `Description`/`TargetFormatDescription`: выполнено
- `GithubProvider.cs` Description + TargetFormatDescription = "<owner>/<repo>": выполнено
- `TelegramClient.cs` Description + TargetFormatDescription = "<chatId> or <chatId>/<threadId>": выполнено
- `GithubIssueEventEntity.cs` — `[FieldDescription]` на всех 10 свойствах: выполнено
- Решение компилируется: выполнено (`dotnet build Issueneter.sln --configuration Release` — 0 warnings, 0 errors)
- Новый `GithubProviderTests.cs` (Type/Description/TargetFormatDescription/GetSample) + дополнения в `TelegramClientTests.cs`: выполнено
- Отдельный интеграционный сегмент не требуется: выполнено (не добавлялся, как и указано в DoD)
- `dotnet test Issueneter.sln --configuration Release`: выполнено (252/252 passed, 0 failed)
- Свойства объявлены как обычные абстрактные свойства интерфейса (не `required`): выполнено
- Нет TODO/FIXME без объяснения: выполнено (новых TODO не добавлено)
