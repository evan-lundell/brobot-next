using System.Text;
using Brobot.Repositories;
using Brobot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TimeZoneConverter;

namespace Brobot.Modules;

public class BrobotModule(
    IUnitOfWork uow,
    Random random,
    IGiphyService giphyService,
    IRandomFactService randomFactService,
    IDictionaryService dictionaryService,
    IHotOpService hotOpService,
    IScheduledMessageService scheduledMessageService,
    IAssemblyService assemblyService,
    ILogger<BrobotModule> logger)
    : InteractionModuleBase
{
    private readonly string[] _emojiLookup =
    [
        ":one:",
        ":two:",
        ":three:",
        ":four:",
        ":five:",
        ":six:",
        ":seven:",
        ":eight:",
        ":nine:"
    ];

    [SlashCommand("info", "Returns guild, channel, and user ids")]
    public async Task Info()
    {
        await RespondAsync($"Guild: {Context.Guild.Id}\nChannel: {Context.Channel.Id}\nUser: {Context.User.Id}");
    }

    [SlashCommand("roll", "Rolls dice based on the given parameters: Ex: /roll 2d6")]
    public async Task Roll([Summary("dice", "The dice to roll in #d# format (ex: 2d6)")] string dice)
    {
        var split = dice.ToLower().Split("d");
        if (split.Length != 2
            || !int.TryParse(split[0], out int numberOfDice)
            || !int.TryParse(split[1], out int diceValue))
        {
            await RespondAsync(text: "Invalid format. Please use #d# (example: 2d6)", ephemeral: true);
            return;
        }

        if (numberOfDice > 25)
        {
            await RespondAsync("Why do you need to roll so many dice? :thinking:");
            return;
        }

        var results = new int[numberOfDice];
        for (int i = 0; i < numberOfDice; i++)
        {
            results[i] = random.Next(1, diceValue + 1);
        }

        await RespondAsync($"{string.Join(", ", results)}\nTotal: {results.Sum()}");
    }

    [SlashCommand("game", "Picks a random game from the given list")]
    public async Task Game(
        string? game1 = null,
        string? game2 = null,
        string? game3 = null,
        string? game4 = null,
        string? game5 = null,
        string? game6 = null,
        string? game7 = null,
        string? game8 = null,
        string? game9 = null,
        string? game10 = null
    )
    {
        var games = new List<string>();
        games.AddIfNotNull(
            game1,
            game2,
            game3,
            game4,
            game5,
            game6,
            game7,
            game8,
            game9,
            game10
        );

        if (games.Count < 2)
        {
            await RespondAsync(text: "Please provide at least 2 games", ephemeral: true);
            return;
        }

        await RespondAsync($"Let's play {games[random.Next(games.Count)]}");
    }

    [SlashCommand("teams", "Generates two teams based on the list of provided players")]
    public async Task Teams(
        string? player1 = null,
        string? player2 = null,
        string? player3 = null,
        string? player4 = null,
        string? player5 = null,
        string? player6 = null,
        string? player7 = null,
        string? player8 = null,
        string? player9 = null,
        string? player10 = null
    )
    {
        var players = new List<string>();
        players.AddIfNotNull(
            player1,
            player2,
            player3,
            player4,
            player5,
            player6,
            player7,
            player8,
            player9,
            player10
        );

        if (players.Count < 2)
        {
            await RespondAsync(text: "Please provide at least 2 players", ephemeral: true);
            return;
        }

        var isTeam1 = true;
        var team1 = new List<string>();
        var team2 = new List<string>();
        var numberOfPlayers = players.Count;
        for (var i = 0; i < numberOfPlayers; i++)
        {
            var randomPlayer = players[random.Next(players.Count)];
            if (isTeam1)
            {
                team1.Add(randomPlayer);
            }
            else
            {
                team2.Add(randomPlayer);
            }
            isTeam1 = !isTeam1;
            players.Remove(randomPlayer);
        }

        await RespondAsync($"{string.Join(", ", team1)}\n{string.Join(", ", team2)}");
    }

    [SlashCommand("gif", "Gets a random gif")]
    public async Task Gif([Summary("tag", "Text for random gif")] string? tag = null)
    {
        try
        {
            var url = await giphyService.GetGif(tag);
            await RespondAsync(url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gif command failed");
            await RespondAsync("An error occurred");
        }
    }

    [SlashCommand("fact", "Gets a random fact")]
    public async Task Fact()
    {
        try
        {
            var fact = await randomFactService.GetFact();
            await RespondAsync(fact);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fact command failed");
            await RespondAsync("An error occurred");
        }
    }

    [SlashCommand("pika", "Posts surprised Pikachu")]
    public async Task Pika()
    {
        await RespondWithFileAsync("./Images/pika.jpeg");
    }

    [SlashCommand("doh", "Posts doh!")]
    public async Task Doh()
    {
        await RespondWithFileAsync("./Images/doh.png");
    }

    [SlashCommand("define", "Gets the definition of a word")]
    public async Task Define(string word)
    {
        try
        {
            var definition = await dictionaryService.GetDefinition(word);
            await RespondAsync(definition);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Define command failed");
            await RespondAsync("An error occurred");
        }
    }

    [SlashCommand("poll", "Creates a poll")]
    public async Task Poll(
        [Summary("question", "The question of the poll")] string question,
        string option1,
        string option2,
        string? option3 = null,
        string? option4 = null,
        string? option5 = null,
        string? option6 = null,
        string? option7 = null,
        string? option8 = null,
        string? option9 = null
    )
    {
        var options = new List<string>
        {
            option1,
            option2
        };
        options.AddIfNotNull(
            option3,
            option4,
            option5,
            option6,
            option7,
            option8,
            option9
        );

        var builder = new StringBuilder();
        for (int i = 0; i < options.Count; i++)
        {
            builder.AppendLine($"{_emojiLookup[i]}: {options[i]}");
        }
        await RespondAsync($"**{question}**\n\n{builder}");
    }

    [SlashCommand("sarcasm", "Reposts text in sarcasm font")]
    public async Task Sarcasm(string text)
    {
        var capitalize = true;
        var builder = new StringBuilder();
        foreach (var c in text)
        {
            if (c is >= 'A' and <= 'z')
            {
                builder.Append(capitalize ? c.ToString().ToUpper() : c.ToString().ToLower());
                capitalize = !capitalize;
            }
            else
            {
                builder.Append(c);
            }
        }

        await RespondAsync(builder.ToString());
    }

    [SlashCommand(name: "reminder", description: "Creates a reminder", runMode: RunMode.Async)]
    public async Task Reminder(
        [Summary("date", "Date and time the reminder will be sent, in yyyy-MM-dd HH:mm format")] string reminderDate,
        [Summary("message", "The message that will be posted")] string message)
    {
        try
        {
            if (!DateTime.TryParse(reminderDate, out DateTime reminderDateFormatted))
            {
                await RespondAsync("Invalid date format. Please use yyyy-MM-dd HH:mm");
                return;
            }

            reminderDateFormatted = DateTime.SpecifyKind(reminderDateFormatted, DateTimeKind.Utc);
            var user = await uow.Users.GetById(Context.User.Id);
            if (user == null)
            {
                await RespondAsync("An error has occured", ephemeral: true);
                return;
            }

            await scheduledMessageService.CreateScheduledMessage(message, user, reminderDateFormatted, Context.Channel.Id);
            await RespondAsync("Reminder has been created");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Reminder message failed");
            await RespondAsync(text: "Failed to create reminder", ephemeral: true);
        }
    }

    [SlashCommand("lastonline", "Get last online times")]
    public async Task LastOnline([Summary("user")] SocketUser socketUser)
    {
        try
        {
            if (socketUser.Status == UserStatus.Online)
            {
                await RespondAsync(text: $"{socketUser.Username} is online now!", ephemeral: true);
                return;
            }

            var user = await uow.Users.GetById(socketUser.Id);
            var callingUser = await uow.Users.GetById(Context.User.Id);
            if (user?.LastOnline == null)
            {
                await RespondAsync(text: "Failed to get last online", ephemeral: true);
                return;
            }

            var lastOnline = user.LastOnline.Value;
            if (!string.IsNullOrWhiteSpace(callingUser?.Timezone))
            {
                var timezone = TZConvert.GetTimeZoneInfo(callingUser.Timezone);
                var offset = timezone.GetUtcOffset(DateTime.Now);
                lastOnline = lastOnline + offset;
            }

            await RespondAsync(text: $"{user.Username} was last online at {lastOnline:yyyy-MM-dd hh:mm tt}", ephemeral: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LastOnline command failed");
            await RespondAsync(text: "Failed to get last online", ephemeral: true);
        }
    }

    [SlashCommand("hotop", "Gets the scores for the active hot ops")]
    public async Task HotOp()
    {
        try
        {
            var activeHotOps = (await uow.HotOps.GetActiveHotOpsWithSessions(Context.Channel.Id)).ToArray();
            if (activeHotOps.Length == 0)
            {
                await RespondAsync("No active hot ops");
                return;
            }

            await RespondAsync(embeds: activeHotOps.Select(hotOpService.CreateScoreboardEmbed).ToArray());

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HotOp command failed");
            await RespondAsync(text: "Failed to get hot op scores", ephemeral: true);
        }
    }

    [SlashCommand("version", "Gets the version of brobot")]
    public async Task Version()
    {
        try
        {
            var version = assemblyService.GetVersionFromAssembly();
            await RespondAsync(text: $"Brobot version: {version}", ephemeral: true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Version command failed");
            await RespondAsync(text: "Failed to get version of brobot", ephemeral: true);
        }
    }
}