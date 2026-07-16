# Brobot

Brobot is a fun Discord bot and companion web app for community engagement, scheduling, and server analytics. It can set reminders, run polls, manage hot ops, track message stats, coordinate Secret Santa groups, and handle other lightweight server utilities.

## Prerequisites

- .NET 10 SDK
- PostgreSQL and Seq running locally, or the development compose stack from this repo
- A GitHub personal access token with access to the private Blazored.Toast package source in `nuget.config`

## Package restore

The solution restores Blazored.Toast from GitHub Packages. If restore fails on a new machine, add your GitHub PAT as a NuGet source credential once:

```bash
dotnet nuget add source https://nuget.pkg.github.com/evan-lundell/index.json \
  --name github \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

The token needs access to the package feed. If the package is private, make sure the PAT has the appropriate package access scope for your account.

## Local setup

1. Start the local services.

```bash
docker compose -f docker-compose.dev.yml up -d
```

1. Restore the solution.

```bash
dotnet restore brobot-next.sln
```

1. Set user secrets for the Brobot project.

```bash
cd src/Brobot
dotnet user-secrets set "Discord:BrobotToken" "YOUR_DISCORD_BOT_TOKEN"
```

At minimum, `Discord:BrobotToken` must be set. The app also validates the rest of the Discord, JWT, and external API settings at startup, so populate the remaining secrets in user-secrets as needed for your environment.

## Running

```bash
dotnet run --project src/Brobot/Brobot.csproj
```

You can disable parts of the app while debugging with:

```bash
dotnet run --project src/Brobot/Brobot.csproj -- --no-bot
dotnet run --project src/Brobot/Brobot.csproj -- --no-jobs
```

## Configuration

- Use `src/Brobot/appsettings.Sample.json` as the starting point for local config shape.
- Keep secrets out of appsettings files and store them with `dotnet user-secrets` instead.