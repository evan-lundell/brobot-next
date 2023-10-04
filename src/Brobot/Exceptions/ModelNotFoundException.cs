namespace Brobot.Exceptions;

public class ModelNotFoundException<TModel, TKey> : ModelNotFoundException
{
    private readonly TKey _id;
    public override string Message => $"{typeof(TModel).Name} {_id} not found";

    public ModelNotFoundException(TKey id)
    {
        _id = id;
    }
}

public abstract class ModelNotFoundException : Exception
{
}