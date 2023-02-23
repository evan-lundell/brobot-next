using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Discord.Interactions;
using TimeZoneConverter;
using System.Text;
using Discord;

namespace Brobot.Modules;

public class BrobotModule : InteractionModuleBase
{
    private readonly IUnitOfWork _uow;
    private readonly Random _random;
    private readonly IGiphyService _giphyService;
    private readonly IRandomFactService _randomFactService;
    private readonly IDictionaryService _dictionaryService;

    private string[] _emojiLookup = new string[]
    {
        ":one:",
        ":two:",
        ":three:",
        ":four:",
        ":five:",
        ":six:",
        ":seven:",
        ":eight:",
        ":nine:"
    };

    public BrobotModule(
        IUnitOfWork uow,
        Random random,
        IGiphyService giphyService,
        IRandomFactService randomFactService,
        IDictionaryService dictionaryService)
    {
        _uow = uow;
        _random = random;
        _giphyService = giphyService;
        _randomFactService = randomFactService;
        _dictionaryService = dictionaryService;
    }
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
            results[i] = _random.Next(1, diceValue + 1);
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
        games.AddIfNotNull(game1);
        games.AddIfNotNull(game2);
        games.AddIfNotNull(game3);
        games.AddIfNotNull(game4);
        games.AddIfNotNull(game5);
        games.AddIfNotNull(game6);
        games.AddIfNotNull(game7);
        games.AddIfNotNull(game8);
        games.AddIfNotNull(game9);
        games.AddIfNotNull(game10);


        if (games.Count < 2)
        {
            await RespondAsync(text: "Please provide at least 2 games", ephemeral: true);
            return;
        }

        await RespondAsync($"Let's play {games[_random.Next(games.Count)]}");
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
        players.AddIfNotNull(player1);
        players.AddIfNotNull(player2);
        players.AddIfNotNull(player3);
        players.AddIfNotNull(player4);
        players.AddIfNotNull(player5);
        players.AddIfNotNull(player6);
        players.AddIfNotNull(player7);
        players.AddIfNotNull(player8);
        players.AddIfNotNull(player9);
        players.AddIfNotNull(player10);

        if (players.Count < 2)
        {
            await RespondAsync(text: "Please provide at least 2 players", ephemeral: true);
            return;
        }

        var isTeam1 = true;
        var team1 = new List<string>();
        var team2 = new List<string>();
        var numberOfPlayers = players.Count;
        for (int i = 0; i < numberOfPlayers; i++)
        {
            var randomPlayer = players[_random.Next(players.Count)];
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
            var url = await _giphyService.GetGif(tag);
            await RespondAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute 'gif' command\n{ex.Message}");
            await RespondAsync("An error occurred");
        }
    }

    [SlashCommand("fact", "Gets a random fact")]
    public async Task Fact()
    {
        try
        {
            var fact = await _randomFactService.GetFact();
            await RespondAsync(fact);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute 'fact' command\n{ex.Message}");
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
            var definition = await _dictionaryService.GetDefinition(word);
            await RespondAsync(definition);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute 'define' command\n{ex.Message}");
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
        options.AddIfNotNull(option3);
        options.AddIfNotNull(option4);
        options.AddIfNotNull(option5);
        options.AddIfNotNull(option6);
        options.AddIfNotNull(option7);
        options.AddIfNotNull(option8);
        options.AddIfNotNull(option9);

        var builder = new StringBuilder();
        for (int i = 0; i < options.Count; i++)
        {
            builder.AppendLine($"{_emojiLookup[i]}: {options[i]}");
        }
        await RespondAsync($"**{question}**\n\n{builder.ToString()}");
    }

    [SlashCommand("sarcasm", "Reposts text in sarcasm font")]
    public async Task Sarcasm(string text)
    {
        var capitalize = true;
        var builder = new StringBuilder();
        foreach (var c in text)
        {
            if (c >= 'A' && c <= 'z')
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
        if (!DateTime.TryParse(reminderDate, out DateTime reminderDateFormatted))
        {
            await RespondAsync("Invalid date format. Please use yyyy-MM-dd HH:mm");
            return;
        }

        reminderDateFormatted = DateTime.SpecifyKind(reminderDateFormatted, DateTimeKind.Utc);
        var user = await _uow.Users.GetById(Context.User.Id);
        var channel = await _uow.Channels.GetById(Context.Channel.Id);
        if (user == null || channel == null)
        {
            await RespondAsync("An error has occured");
            return;
        }
        if (!string.IsNullOrWhiteSpace(user.Timezone))
        {
            var timezone = TZConvert.GetTimeZoneInfo(user.Timezone);
            if (timezone != null)
            {
                var offset = timezone.GetUtcOffset(DateTime.Now);
                reminderDateFormatted = reminderDateFormatted - offset;
            }
        }

        var reminder = new ScheduledMessageModel
        {
            MessageText = message,
            SendDate = reminderDateFormatted,
            Channel = channel,
            ChannelId = Context.Channel.Id,
            CreatedBy = user,
            CreatedById = Context.User.Id
        };

        await _uow.ScheduledMessages.Add(reminder);
        await _uow.CompleteAsync();
        await RespondAsync("Reminder has been created");
    }

    [SlashCommand("hotop", "Gets the scores for the active hot ops")]
    public async Task HotOp()
    {
        try
        {
            var scoreboards = await _uow.HotOps.GetActiveHotOpScoreboards();
            if (scoreboards.Count() == 0)
            {
                await RespondAsync("No active hot ops");
                return;
            }

            var embeds = new List<Embed>();
            foreach (var scoreboard in scoreboards)
            {
                var builder = new EmbedBuilder
                {
                    Color = new Color(114, 137, 218),
                    Description = $"Operation Hot {scoreboard.OwnerUsername}"

                };

                foreach (var score in scoreboard.Scores)
                {
                    builder.AddField((x) =>
                    {
                        x.Name = score.Username;
                        x.Value = score.Score;
                        x.IsInline = false;
                    });
                }

                embeds.Add(builder.Build());
            }

            await RespondAsync(embeds: embeds.ToArray());

        }
        catch (Exception)
        {
            await RespondAsync(text: "Failed to get hot op scores", ephemeral: true);
        }
    }
}