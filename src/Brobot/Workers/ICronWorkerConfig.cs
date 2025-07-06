namespace Brobot.Workers;

// ReSharper disable once UnusedTypeParameter
public interface ICronWorkerConfig<T> where T : CronWorkerBase
{
    string? CronExpression { get; set; }
}

public class CronWorkerConfig<T> : ICronWorkerConfig<T> where T : CronWorkerBase
{
    public string? CronExpression { get; set; }
}