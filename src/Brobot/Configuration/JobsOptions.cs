namespace Brobot.Configuration;

public class JobsOptions
{
    public const string SectionName = "Jobs";

    public string ReminderCron { get; init; } = "* * * * *";
    public string BirthdayCron { get; init; } = "0 12 * * *";
    public string HotOpCron { get; init; } = "* * * * *";
    public string MonthlyStatsCron { get; init; } = "0 12 1 * *";
}