# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Brobot is a Discord bot and companion Blazor WebAssembly web app for community engagement, scheduling, and server analytics (reminders, polls, hot ops, message stats, Secret Santa, birthdays, etc). The bot and web API run as a single ASP.NET Core host; the frontend is a separate Blazor WASM app served by that host.

## Solution layout

- `src/Brobot` — the ASP.NET Core host. Contains the Discord bot (Discord.Net), REST API controllers, EF Core data access, hosted background workers, and serves the compiled `Brobot.Frontend` WASM app as static files.
- `src/Brobot.Frontend` — Blazor WebAssembly SPA consumed by the browser. Talks to `src/Brobot`'s controllers over HTTP via `ApiService`.
- `src/Brobot.Shared` — DTOs/requests/responses/claim types shared between the API and the frontend (referenced by both).
- `tests/Brobot.Tests` — NUnit test project for `src/Brobot` only (no frontend test project exists).

## Commands

Local services (Postgres + Seq) via Docker:

```bash
docker compose -f docker-compose.dev.yml up -d
```

Restore/build/test the whole solution:

```bash
dotnet restore brobot-next.sln
dotnet build brobot-next.sln
dotnet test tests/Brobot.Tests/Brobot.Tests.csproj
```

Run a single test (NUnit, by fully-qualified name or a filter expression):

```bash
dotnet test tests/Brobot.Tests/Brobot.Tests.csproj --filter "FullyQualifiedName~SyncServiceTests.MessageReceivedTests"
dotnet test tests/Brobot.Tests/Brobot.Tests.csproj --filter "Name=SomeSpecificTestMethod"
```

Run the app:

```bash
dotnet run --project src/Brobot/Brobot.csproj
```

Useful flags for local debugging (checked against `args` in `Program.Main`):

```bash
dotnet run --project src/Brobot/Brobot.csproj -- --no-bot   # skip Discord client/bot startup
dotnet run --project src/Brobot/Brobot.csproj -- --no-jobs  # skip cron background jobs
```

Package restore requires a GitHub Packages source for the private `Blazored.Toast` dependency — see README.md for the `dotnet nuget add source` one-time setup.

EF Core migrations live in `src/Brobot/Migrations` and use the `brobot` schema with a custom history table name (`__EFMigrationsHistory`) — see `AddBrobotInfrastructure` in `ServiceCollectionExtensions.cs`. Migrations are applied automatically at startup by `MigrationsHostedService`, not run manually.

Config: copy `src/Brobot/appsettings.Sample.json` as the shape reference; set secrets (Discord token, JWT signing key, external API keys, connection string) with `dotnet user-secrets` in `src/Brobot`, never in committed appsettings files. All options sections are validated on startup via `ValidateDataAnnotations().ValidateOnStart()` (see `AddBrobotOptions`) — a missing/invalid required setting fails fast at boot rather than at first use.

CI (`.github/workflows`): PRs to `main` run `dotnet restore` / `dotnet build --configuration Release` / `dotnet test`. Pushing a `v*` tag builds and pushes a multi-arch Docker image and deploys it to the home server over SSH.

## Architecture

### Two entry points sharing one host

`Program.CreateServices` wires up everything unconditionally needed (EF Core, identity/JWT auth, Swagger, hosted services) and then conditionally adds two independent subsystems based on CLI args:

- `AddJobs` — registers the cron-based `IHostedService` workers (`ReminderWorker`, `BirthdayWorker`, `HotOpWorker`, `MonthlyStatsWorker`), each configured with its own cron expression from `JobsOptions`.
- `AddDiscord` — registers the `DiscordSocketClient` and `DiscordBotHostedService`, which starts the gateway connection and calls `DiscordEventHandler.RegisterEvents()`.

Both can be disabled independently (`--no-jobs`, `--no-bot`) for local debugging without needing a live bot token or wanting jobs to fire.

### Discord event flow: handler → queue → sync/module

`DiscordEventHandler` subscribes to every `DiscordSocketClient` event (message received/deleted, channel/guild/thread lifecycle, voice/presence updates, interactions). Handlers do **not** do work inline — they push a closure onto `IBackgroundTaskQueue` (an unbounded `System.Threading.Channels` queue) and return immediately, so the Discord gateway thread is never blocked. `QueuedHostedService` (registered in `AddBrobotInfrastructure`) drains that queue and awaits each work item sequentially.

- Raw Discord gateway events (guild/channel/message/thread/presence sync) are handled by `ISyncService`, which mirrors Discord state into the EF Core database (`GuildModel`, `ChannelModel`, `DiscordUserModel`, message counts, etc).
- Slash commands are handled by Discord.Net's `InteractionService`. Commands live in `Modules/BrobotModule.cs` as one method per `[SlashCommand]`, using constructor-injected services (DI works per-invocation since `InteractionModuleBase` instances are created fresh per interaction).
- The one-time `Ready` handler bootstraps interaction modules, registers slash commands globally, runs `SyncService.SyncOnStartup`, and checks for a version update — guarded by an `Interlocked` flag so it only runs once even if `Ready` fires multiple times.

### Data access: repository + unit of work over EF Core

`BrobotDbContext` (in `Contexts/`) is the single EF Core context. Access goes through `IUnitOfWork`, which exposes one repository property per aggregate (`Users`, `Guilds`, `Channels`, `HotOps`, `ScheduledMessages`, `SecretSantaGroups`, `StopWords`, `DailyMessageCounts`, `StatPeriods`, `Versions`). Repositories derive from `RepositoryBase<TEntity, TKey>` for common CRUD (`Add`, `Find`, `GetById`, `Remove`, ...) and add entity-specific query methods on top. `IUnitOfWork.CompleteAsync` persists changes; `BeginTransaction`/`CommitTransaction` are available for multi-step operations that need atomicity.

Controllers and services never talk to `BrobotDbContext` directly — they depend on `IUnitOfWork` (and specific repository interfaces), which keeps EF Core out of the API/Discord-facing layers and is what makes the in-memory-DB test setup (see below) straightforward.

`DiscordUserMiddleware` runs after authentication and, if the JWT's Discord ID claim resolves to a known user, stashes that `DiscordUserModel` on `HttpContext.Features` for controllers to read — this is how authenticated web requests get the current Discord identity without a second lookup.

### Web API ↔ frontend contract

`src/Brobot.Shared` is the only thing both the ASP.NET host and the Blazor WASM app reference. Controllers in `src/Brobot/Controllers` accept `Brobot.Shared.Requests.*` and return `Brobot.Shared.Responses.*`; `Mappers/*MappingExtensions.cs` convert between EF `Models/*` entities and those shared DTOs. The frontend's `Services/*Service.cs` classes (e.g. `HotOpService`, `SecretSantaService`) wrap `ApiService`/`ApiServiceBase` to call those same controllers and deserialize the same response types — when changing a controller's request/response shape, update the corresponding type in `Brobot.Shared` and both sides stay in sync.

Auth on the frontend is JWT-based: `JwtService` stores/refreshes the token, `JwtTokenMessageHandler` attaches it to outgoing `HttpClient` requests, and `JwtAuthenticationStateProvider` feeds Blazor's `AuthenticationStateProvider` from the token's claims.

### Cron workers

`CronWorkerBase` is a generic `IHostedService` that parses a cron expression (Cronos) and self-reschedules a one-shot `Timer` after each run (adding a 1s buffer to avoid firing early) rather than using a fixed interval — subclasses only implement `DoWork`. Each worker (`ReminderWorker`, `BirthdayWorker`, `HotOpWorker`, `MonthlyStatsWorker`) resolves its own scoped dependencies per run since the host itself is a singleton.

### Testing conventions

- NUnit, with `Moq` for mocking and `Microsoft.EntityFrameworkCore.InMemory` for a real (non-mocked) `BrobotDbContext` per test — see `SyncServiceTestsBase` for the canonical pattern: build a fresh `ServiceProvider` with a uniquely-named in-memory database per test, seed via an abstract `SetupDatabase()`, and tear down with `Context.Database.EnsureDeleted()`.
- Test classes are organized as `<Thing>Tests/<Method>Tests.cs` with a shared `<Thing>TestsBase` per folder (e.g. `SyncServiceTests/`, `HotOpServiceTests/`, `MessageCountServiceTests/`, `ScheduledMessageServiceTests/`, `SecretSantaServiceTests/`) rather than one file per class under test.
- Services that touch `IServiceScopeFactory` (to create their own DI scope per background operation) are tested by mocking the scope factory to return the real in-memory-DB-backed service provider — this lets tests exercise real repository/EF Core behavior while still mocking external boundaries (Discord client, HTTP clients, logger).
