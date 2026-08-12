# План: Telegram `help` команда

Создано: 2026-08-12
Контекст/цель: У бота Issueneter сейчас нет встроенной документации по
командам/провайдерам/клиентам — пользователь должен читать исходники, чтобы
узнать, какие аргументы принимает `create`, в каком формате указывать target
для GitHub-провайдера или какие поля есть у сущности, которую он возвращает.
Добавляем команду `help`:

1. `help` — список всех команд с кратким описанием.
2. `help command <name>` — полное описание команды, её аргументы
   (имя/описание/обязательность) и пример использования.
3. `help provider <name>` — описание провайдера, формат target'а и поля
   модели сущности, которую он возвращает (имя, тип, опционально описание).
4. `help client <name>` — описание клиента и формат target'а.
5. `help provider` / `help client` без имени — список всех
   зарегистрированных провайдеров/клиентов.

Подтверждённые пользователем решения:
- Описания команд/провайдеров/клиентов обязаны быть частью интерфейса —
  отсутствие описания должно быть ошибкой компиляции, а не рантайма.
- Новые команды/провайдеры/клиенты подхватываются `help` автоматически, без
  дополнительной регистрации сверх того, что и так нужно для их добавления.
- Список полей модели провайдера получается тем же reflection-механизмом,
  что уже использует filter DSL над `Entity` (`GetType().GetProperty(name,
  BindingFlags...)`), а не чем-то новым и непохожим. Доп. атрибут для
  человекочитаемых описаний полей — опционален.
- Синтаксис в чате — буквальный: `help command create`, `help provider
  Github` (обычными словами), а не `Key: Value` строки, хотя `CommandParser`
  сегодня разбирает в первой строке только `<name> [<numeric workerId>]`.

Ключевые архитектурные решения (полный текст — в исходном одобренном плане,
сохранён также в истории сессии):
- Compile-enforced описания — обычные abstract-свойства без реализации по
  умолчанию (`string Description { get; }`), НЕ модификатор `required` (он
  требует settable-свойство и не подходит для expression-bodied `Type =>
  ProviderType.Github` паттерна, уже используемого в проекте).
- `CommandParser` меняется точечно: когда первая строка начинается с "help"
  и не проходит существующий regex `^(?<name>[a-zA-Z]+)( (?<workerId>[0-9]+))?$`,
  добавляется fallback-regex, который кладёт до двух доп. токенов в
  `Parameters["Type"]`/`Parameters["Name"]` существующего словаря — БЕЗ
  изменения `Command` record и БЕЗ влияния на разбор остальных команд.
- DI-цикл: `HelpCommandHandler` НЕ должен конструкторно инжектить
  `IEnumerable<ICommandHandler>` (это тот же список, в который сам
  регистрируется handler — циклическая зависимость в DI). Вместо этого —
  инжектить `IServiceProvider` и резолвить `GetServices<ICommandHandler>()`
  лениво внутри `Handle()`. `IEnumerable<IEntityProvider>` /
  `IEnumerable<IClient>` можно инжектить в конструктор напрямую — там цикла
  нет.
- `Entity.GetFields()` использует `BindingFlags.Public | BindingFlags.Instance`
  (как `Entity.HasProperty`, не как `Entity.GetProperty` с `NonPublic`),
  чтобы help показывал ровно то же поле-пространство, что доступно
  фильтрам/шаблонам.
- Провайдеры/клиенты остаются идентифицируемы по enum (`ProviderType`/
  `ClientType`) — как и сегодня в `CreateCommandHandler`
  (`EnumExtensions.TryParseSafe<T>`), без добавления отдельного `Name`.

---

## Шаг 1: Domain-модели и атрибут для описания полей

- id: 1
- depends_on: []
- risk: low
- status: done
- checkpoint: n/a
- dod: |
    - Новый `Issueneter.Domain/Models/CommandArgumentInfo.cs`:
      `public record CommandArgumentInfo(string Name, string Description, bool IsRequired);`
    - Новый `Issueneter.Domain/Attributes/FieldDescriptionAttribute.cs`
      (`AttributeTargets.Property`, sealed, конструктор `(string description)`).
    - Новый `Issueneter.Domain/Models/EntityFieldInfo.cs`:
      `public record EntityFieldInfo(string Name, Type PropertyType, string? Description);`
      (имя поля `PropertyType`, НЕ `Type` — не должно затенять `System.Type`
      рядом с `ProviderType`/`ClientType` в остальном коде).
    - `Entity.GetFields()` в `Issueneter.Domain/Models/Entity.cs`: рефлексия
      `GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)`,
      читает `FieldDescriptionAttribute` через `GetCustomAttribute<>()`.
    - Юнит-тесты (новый `tests/Issueneter.Tests/Unit/Models/EntityTests.cs`):
      `GetFields()` возвращает все public instance свойства; `Description`
      равен `null`, если атрибута нет; равен тексту атрибута, если он есть.
      Используется отдельная тестовая сущность-фикстура, НЕ общий
      `TestEntity`, используемый в `WorkerConfigurationValidatorTests`/тестах
      фильтров (чтобы не менять его смысл для несвязанных тестов).
    - Никакой другой код пока не меняется — эти типы ещё не используются.

## Шаг 2: Контракт `ICommandHandler` + существующие обработчики команд

- id: 2
- depends_on: [1]
- risk: high
- status: done
- checkpoint: passed
- dod: |
    - В `Issueneter.Domain.Interfaces/Commands/ICommandHandler.cs` добавлены
      `string Name { get; }`, `string Description { get; }`,
      `IReadOnlyCollection<CommandArgumentInfo> Arguments { get; }`,
      `string Example { get; }` — обычные abstract-свойства, без `required`.
    - Обновлены (реализуют новые члены) `CreateCommandHandler`,
      `UpdateCommandHandler`, `GetCommandHandler`, `DeleteCommandHandler`:
      - `CreateCommandHandler`: `Arguments` — все 7 полей `CreateCommand`
        (`ProviderType`, `ProviderTarget`, `Schedule`, `Filter`,
        `ClientType`, `ClientTarget`, `Template`), все `IsRequired: true`
        (соответствует `CreateCommand.Parse`).
      - `UpdateCommandHandler`: `Arguments` — `ProviderTarget`, `Schedule`,
        `Filter`, `ClientTarget`, `Template`, все `IsRequired: false`
        (соответствует `UpdateCommand.Parse`, минимум одно обязательно —
        это в `Description`, не в отдельном поле).
      - `GetCommandHandler`/`DeleteCommandHandler`: `Arguments => []`
        (единственный вход — `WorkerId` в первой строке, `Parameters`
        должны быть пустыми — это уже проверяется в `Handle`).
    - Решение всего решения (`dotnet build`) компилируется.
    - Юнит-тесты: в существующих `CreateCommandHandlerTests`,
      `UpdateCommandHandlerTests`, `GetCommandHandlerTests` — добавлены
      проверки `Name`/`Description`/`Arguments`/`Example` (непустые,
      `Arguments` соответствуют реальным полям DTO). Новый
      `DeleteCommandHandlerTests.cs` (файла ещё не существует) — по
      аналогии с `GetCommandHandlerTests.cs`, плюс те же проверки метаданных.
    - Отдельный интеграционный сегмент не требуется: изменение контракта
      обеспечено компилятором (CS0535 при отсутствии реализации у любого
      `ICommandHandler`) и покрыто юнит-тестами каждой реализации; реальный
      рантайм-риск (DI резолюция обработчиков) вынесен в шаг 6.

## Шаг 3: Контракт `IEntityProvider`/`IClient` + GithubProvider/TelegramClient

- id: 3
- depends_on: [1]
- risk: high
- status: done
- checkpoint: passed
- dod: |
    - В `IEntityProvider` и `IClient`
      (`Issueneter.Domain.Interfaces/Services/`) добавлены
      `string Description { get; }`, `string TargetFormatDescription { get; }`.
    - `GithubProvider.cs`: `Description` (что делает — опрашивает issue
      events GitHub-репозитория), `TargetFormatDescription` — тот же текст,
      что уже используется в `Validate()`: `"<owner>/<repo>"`.
    - `TelegramClient.cs`: `Description` (отправляет сообщение в
      Telegram-чат/топик через Bot API), `TargetFormatDescription` — текст
      из существующего `Validate()`: `"<chatId> or <chatId>/<threadId>"`.
    - `GithubIssueEventEntity.cs`: `[FieldDescription("...")]` на всех 10
      свойствах (`Id`, `Event`, `Author`, `Title`, `Body`, `Url`,
      `CreatedAt`, `UpdatedAt`, `Labels`, `ChangedLabels`) — первое реальное
      использование атрибута из шага 1.
    - Решение компилируется.
    - Юнит-тесты: новый `tests/Issueneter.Tests/Unit/Github/GithubProviderTests.cs`
      (файла ещё не существует) — `Type`, `Description`,
      `TargetFormatDescription`, `GetSample`. В `TelegramClientTests.cs` —
      добавлены проверки `Description`/`TargetFormatDescription`.
    - Отдельный интеграционный сегмент не требуется по той же причине, что
      в шаге 2 (компилятор + юнит-тесты); DI-риск — в шаге 6.

## Шаг 4: `CommandParser` — буквальный синтаксис `help <type> <name>`

- id: 4
- depends_on: []
- risk: low
- status: done
- checkpoint: n/a
- dod: |
    - В `CommandParser.cs` добавлен fallback: если первая строка начинается
      с "help" (case-insensitive) и не проходит существующий
      `CommandNameRegex`, применяется новый regex
      `^help(?:\s+(?<type>[a-zA-Z]+))?(?:\s+(?<name>\S+))?$` (IgnoreCase),
      результат кладётся в `Parameters["Type"]`/`Parameters["Name"]`
      (WorkerId остаётся `WorkerId.Empty`). Цикл разбора `Key: Value` строк
      после первой строки — общий, без дублирования логики.
    - Разбор ЛЮБОЙ другой команды не меняется: тот же regex, тот же путь
      кода, тот же текст ошибок (например, `"create abc"` по-прежнему
      падает с `"Invalid command name: create abc"`).
    - Юнит-тесты в `CommandParserTests.cs`: `help`, `help 123` (уже
      покрыто существующим regex — добавить явный тест, чтобы
      зафиксировать поведение), `help command create`, `help provider
      Github`, `help provider` (только Type, без Name), `help provider
      Github extra` (4 токена — должно упасть с `Invalid command name`).
    - Эта команда пока никем не используется (`HelpCommandHandler` ещё
      нет) — шаг самодостаточен и проверяется только через
      `CommandParser`/`Command`.

## Шаг 5: `HelpFormatter` + `HelpCommandHandler` + регистрация в DI

- id: 5
- depends_on: [1, 2, 3, 4]
- risk: high
- status: pending
- checkpoint: pending
- dod: |
    - Новый `Issueneter.Application/Commands/HelpFormatter.cs`
      (`internal static class`) — чистые функции без DI:
      `FormatCommandList`, `FormatCommandDetail`, `FormatProviderList`,
      `FormatProviderDetail` (вызывает `provider.GetSample().GetFields()`),
      `FormatClientList`, `FormatClientDetail`, приватный хелпер
      `FriendlyTypeName(Type)` (generic-типы вида `IReadOnlyCollection<string>`
      вместо CLR-имени `` IReadOnlyCollection`1 ``).
    - Новый `Issueneter.Application/Commands/Handlers/HelpCommandHandler.cs`:
      конструктор — `IServiceProvider`, `IEnumerable<IEntityProvider>`,
      `IEnumerable<IClient>` (**НЕ** `IEnumerable<ICommandHandler>` — см.
      DI-цикл в контексте плана). `Name => "help"`. `CanHandle` сравнивает
      `command.Name` с `"help"` case-insensitive. `Handle()` читает
      `Type`/`Name` из `command.Parameters`, резолвит
      `IEnumerable<ICommandHandler>` через
      `_serviceProvider.GetServices<ICommandHandler>()` ТОЛЬКО внутри
      `Handle` (не в конструкторе), диспатчит: без Type → список команд;
      Type=="command" (+Name опционален) → список или детальное описание /
      "не найдено"; Type=="provider"/"client" аналогично, через
      `EnumExtensions.TryParseSafe<ProviderType/ClientType>` + поиск по
      `.Type` в инжектированных коллекциях; неизвестный Type → сообщение об
      ошибке.
    - `ServiceCollectionExtensions.AddCommandHandlers()` — добавлена
      `.AddScoped<ICommandHandler, HelpCommandHandler>()`.
    - Решение компилируется, весь существующий тестовый набор зелёный.
    - Юнит-тесты: новый `HelpCommandHandlerTests.cs` (список/детали для
      команд/провайдеров/клиентов, включая "не найдено" и "неизвестный
      Type"; `IServiceProvider` мокается так, чтобы
      `GetServices<ICommandHandler>()` возвращал фиксированный список —
      через `Mock<IServiceProvider>` с настройкой
      `GetService(typeof(IEnumerable<ICommandHandler>))`, либо через
      реальный `ServiceCollection`/`BuildServiceProvider()` с
      зарегистрированными моками обработчиков). Новый
      `HelpFormatterTests.cs` — чистые тесты форматирования на
      `Mock<ICommandHandler>`/`Mock<IEntityProvider>`/`Mock<IClient>` и
      небольшой локальной `Entity`-фикстуре с `[FieldDescription]`.

## Шаг 6: Интеграционный тест — DI-резолюция `help` без цикла

- id: 6
- depends_on: [5]
- risk: low
- status: pending
- checkpoint: n/a
- dod: |
    Реальная (без моков DI-контейнера) проверка того, что описанный в
    контексте плана DI-цикл действительно не возникает: новый тест
    строит настоящий `ServiceCollection`, вызывает
    `.AddApplicationServices()` (как в composition root), создаёт scope,
    резолвит `ICommandHandlerFactory` и вызывает
    `Get(new Command("help", WorkerId.Empty, new Dictionary<string,string>()))` —
    ожидается ненулевой `HelpCommandHandler` и отсутствие исключений
    (в частности, никакого `InvalidOperationException` о циклической
    зависимости). Регистрация `IEntityProvider`/`IClient` из Infrastructure
    для этого теста не нужна — проверяется именно контракт
    `ICommandHandler`-графа, не полный DI-граф приложения.

---

## Пояснения по полям

- `risk: high` — если шаг меняет контракт между модулями, схему данных,
  используемую другими модулями, или добавляет внешнюю интеграцию.
  Для таких шагов `checkpoint` обязателен: `pending` до прохождения
  `architecture-checkpoint`, затем `passed`.
- `checkpoint: n/a` — для `risk: low` шагов чекпоинт не требуется.
- `status` — см. docs/plans/README.md, раздел "Статусы шага".
