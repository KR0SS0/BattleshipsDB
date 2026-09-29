namespace Battleship.Domain;

public sealed class Result<T>
{
    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess => _error is null;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read Value of a failed result.");

    public Error Error => _error
        ?? throw new InvalidOperationException("Cannot read Error of a successful result.");

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(default, error);
    }
}

public sealed class Result
{
    private Result(Error? error)
    {
        _error = error;
    }

    private readonly Error? _error;

    public bool IsSuccess => _error is null;

    public Error Error => _error
        ?? throw new InvalidOperationException("Cannot read Error of a successful result.");

    public static Result Success() => new(null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }
}