namespace Brobot.Exceptions;

public class ModelNotFoundException<TModel, TKey>(TKey id) : ModelNotFoundException
{
    public override string Message => $"{typeof(TModel).Name} {id} not found";
}

public abstract class ModelNotFoundException : Exception;